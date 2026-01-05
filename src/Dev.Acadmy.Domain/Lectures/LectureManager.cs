using AutoMapper;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Questions;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;
namespace Dev.Acadmy.Lectures
{
    public class LectureManager:DomainService
    {
        private readonly IRepository<Lecture,Guid> _lectureRepository;
        private readonly IMapper _mapper;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly QuizManager _quizManager;
        private readonly IQuizRepository _quizRepository;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IRepository<LectureStudent ,Guid> _lectureStudentRepository;
        private readonly IRepository<LectureTry , Guid> _lectureTryRepository;
        private readonly IRepository<QuizStudent, Guid> _quizStudentRepository;
        private readonly IRepository<Question, Guid> _questionRepository;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IExamQuestionRepository _examQuestionRepository;
        private readonly IRepository<Exam, Guid> _examRepository;
        public LectureManager(IRepository<Exam, Guid> examRepository, IExamQuestionRepository examQuestionRepository, ICourseRepository courseRepository, IMediaItemRepository mediaItemRepository, IRepository<Question, Guid> questionRepository, IRepository<QuizStudent, Guid> quizStudentRepository, IRepository<LectureTry, Guid> lectureTryRepository, IRepository<LectureStudent, Guid> lectureStudentRepository, MediaItemManager mediaItemManager, IQuizRepository quizRepository, QuizManager quizManager, ICurrentUser currentUser, IIdentityUserRepository userRepository, IMapper mapper, IRepository<Lecture,Guid> lectureRepository)
        {
            _examRepository = examRepository;
            _examQuestionRepository = examQuestionRepository;
            _courseRepository = courseRepository;
            _mediaItemRepository = mediaItemRepository;
            _questionRepository = questionRepository;
            _quizStudentRepository = quizStudentRepository;
            _lectureTryRepository = lectureTryRepository;
            _lectureStudentRepository = lectureStudentRepository;
            _mediaItemManager = mediaItemManager;
            _quizRepository = quizRepository;
            _quizManager = quizManager;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _lectureRepository = lectureRepository;
            _mapper = mapper;
        }

        public async Task<ResponseApi<LectureDto>> GetAsync(Guid id)
        {
            var lecture = await (await _lectureRepository.GetQueryableAsync()).Include(x=>x.Quizzes).Include(x=>x.Chapter).FirstOrDefaultAsync(x => x.Id == id);
            if (lecture == null) return new ResponseApi<LectureDto> { Data = null, Success = false, Message = "Not found lecture" };
            var dto = _mapper.Map<LectureDto>(lecture);
            var lecPdfs = await _mediaItemManager.GetListAsync(id);
            foreach(var pdf in lecPdfs) if (!pdf.IsImage) dto.PdfUrls.Add(pdf.Url);
            dto.QuizCount = lecture.Quizzes.Count();
            dto.CourseId = lecture.Chapter.CourseId;
            dto.QuizTime = lecture?.Quizzes?.FirstOrDefault()?.QuizTime?? 0;
            dto.QuizTryCount = lecture?.QuizTryCount??0;
            dto.IsRequiredQuiz = lecture?.IsRequiredQuiz?? false;
            dto.IsFree = lecture?.IsFree ?? false;
            dto.IsVisible = lecture?.IsVisible ?? false;
            
            return new ResponseApi<LectureDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<LectureDto>> GetListAsync(int pageNumber, int pageSize, string? search, Guid chapterId)
        {
            var currentUserId = _currentUser.GetId();
            var roles = await _userRepository.GetRolesAsync(currentUserId);
            var isAdmin = roles.Any(x => x.Name.Equals(RoleConsts.Admin, StringComparison.OrdinalIgnoreCase));

            var queryable = await _lectureRepository.GetQueryableAsync();

            // 1. الفلترة الأساسية (حسب الفصل والصلاحيات)
            queryable = queryable.Where(x => x.ChapterId == chapterId);

            if (!isAdmin)
            {
                queryable = queryable.Where(c => c.CreatorId == currentUserId);
            }

            // 2. البحث في المحتوى (Content) فقط
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                queryable = queryable.Where(c => c.Content.ToLower().Contains(searchLower));
            }

            // 3. تحميل العلاقات المطلوبة للـ DTO
            queryable = queryable.Include(x => x.Chapter).ThenInclude(x => x.Course)
                                 .Include(x => x.Quizzes);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // 4. جلب البيانات الأساسية (Pagination)
            var lectures = await AsyncExecuter.ToListAsync(
                queryable.OrderByDescending(c => c.CreationTime)
                         .Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
            );

            // --- حل مشكلة الـ N+1 لجلب الـ PDFs في استعلام واحد (Batch Loading) ---
            var lectureIds = lectures.Select(x => x.Id).ToList();
            var allMediaItems = await _mediaItemRepository.GetListAsync(x => lectureIds.Contains(x.RefId));
            var mediaLookup = allMediaItems.ToLookup(x => x.RefId);

            // 5. التحويل إلى DTO
            var lectureDtos = lectures.Select(l => new LectureDto
            {
                Id = l.Id,
                ChapterId = l.ChapterId,
                ChapterName = l.Chapter?.Name?? string.Empty,
                Content = l.Content,
                Title = l.Title,
                YouTubeVideoUrl = l.YouTubeVideoUrl,
                DriveVideoUrl = l.DriveVideoUrl,
                HasYouTubeVideo = l.HasYouTubeVideo,
                HasDriveVideo = l.HasDriveVideo,
                HasTelegramVideo = l.HasTelegramVideo,
                TelegramVideoUrl = l.TelegramVideoUrl,
                CourseId = l.Chapter?.CourseId ?? Guid.Empty,
                CourseName = l.Chapter?.Course?.Name?? string.Empty,
                IsVisible = l.IsVisible,
                QuizCount = l.Quizzes.Count,
                QuizTime = l.Quizzes.FirstOrDefault()?.QuizTime ?? 0,
                QuizTryCount = (l.QuizTryCount * l.Quizzes.Count),
                IsFree = l.IsFree,
                IsRequiredQuiz = l.IsRequiredQuiz,
                // جلب الروابط من الـ Lookup الموجود في الذاكرة
                PdfUrls = mediaLookup[l.Id]
                            .Where(m => !m.IsImage)
                            .Select(m => m.Url)
                            .ToList()
            }).ToList();

            return new PagedResultDto<LectureDto>(totalCount, lectureDtos);
        }

        public async Task<ResponseApi<LectureDto>> CreateAsync(CreateUpdateLectureDto input)
        {
            var lecture = _mapper.Map<Lecture>(input);
            var result = await _lectureRepository.InsertAsync(lecture);
            await CreateQuizes(input, result.Id);
            foreach (var pdfUrl in input.PdfUrls) await _mediaItemManager.CreateAsync(new CreateUpdateMediaItemDto { IsImage = false, RefId = result.Id, Url = pdfUrl });
            var dto = _mapper.Map<LectureDto>(result);
            return new ResponseApi<LectureDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<LectureDto>> UpdateAsync(Guid id, CreateUpdateLectureDto input)
        {
            var lectureDB = await _lectureRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (lectureDB == null) return new ResponseApi<LectureDto> { Data = null, Success = false, Message = "Not found lecture" };
            var lecture = _mapper.Map(input, lectureDB);
            var result = await _lectureRepository.UpdateAsync(lecture);
            await _mediaItemManager.DeleteManyAsync(id);
            foreach (var pdfUrl in input.PdfUrls) await _mediaItemManager.CreateAsync(new CreateUpdateMediaItemDto { IsImage = false, RefId = result.Id, Url = pdfUrl });
            var dto = _mapper.Map<LectureDto>(result);
            return new ResponseApi<LectureDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var lecture = await _lectureRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (lecture == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found lecture" };
            await _mediaItemManager.DeleteAsync(id);
            await _quizManager.DeletQuizesByLectureId(id);
            await _lectureStudentRepository.DeleteManyAsync( await (await _lectureStudentRepository.GetQueryableAsync()).Where(x => x.LectureId == id).ToListAsync());
            await _lectureRepository.DeleteAsync(lecture);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task CreateQuizes(CreateUpdateLectureDto input , Guid lectureId)
        {
            if(input.QuizCount > 10) throw new UserFriendlyException("You can create up to 10 quizzes per lecture.");
            for (var i = 0; i < input.QuizCount; i++) await _quizManager.CreateAsync(new CreateUpdateQuizDto {QuizTryCount=input.QuizTryCount, CreaterId = _currentUser.GetId(), QuizTime = input.QuizTime, LectureId = lectureId,Title = input.Title+" " + "Quiz " + i, Description = input.Content });
        }
        // here
        public async Task<ResponseApi<QuizDetailsDto>> GetQuizDetailsAsync(Guid refId, bool isExam)
        {
            // متغيرات لحمل البيانات الأساسية بغض النظر عن المصدر
            Guid finalId;
            string finalTitle;
            int finalQuizTime;
            int finalTryCount;
            List<Question> targetQuestions;

            if (isExam)
            {
                // 1. جلب بيانات الامتحان من مستودع الامتحانات (افترضت وجود IExamRepository)
                var exam = await _examRepository.GetAsync(refId);
                if (exam == null) throw new UserFriendlyException("Exam not found");

                finalId = exam.Id;
                finalTitle = exam.Name;
                finalQuizTime = exam.TimeExam;
                finalTryCount = 0; // القيمة من جدول الامتحان مباشرة

                // 2. جلب أسئلة الامتحان
                var examData = await _examQuestionRepository.GetQuestionsByExamIdAsync(refId);
                targetQuestions = examData.Select(x => x.Question).ToList();
            }
            else
            {
                // 1. جلب بيانات الكويز مع المحاضرة
                var quizFull = await _quizRepository.GetQuizWithQuestionsAsync(refId);
                if (quizFull == null) throw new UserFriendlyException("Quiz not found");

                finalId = quizFull.Id;
                finalTitle = quizFull.Title;
                finalQuizTime = quizFull.QuizTime;
                finalTryCount = quizFull?.Lecture?.QuizTryCount ?? 0; // القيمة من المحاضرة المرتبطة

                targetQuestions = quizFull?.Questions?.ToList() ?? new List<Question>();
            }

            // 3. جلب الصور دفعة واحدة (Batch Fetching) لتحسين الأداء
            var questionIds = targetQuestions.Select(q => q.Id).ToList();
            var imagesDict = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(questionIds);

            // 4. بناء الـ DTO الموحد
            var dto = new QuizDetailsDto
            {
                QuizId = finalId,
                Title = finalTitle,
                QuizTime = finalQuizTime,
                QuizTryCount = finalTryCount,
                Questions = targetQuestions.Select(q => new QuestionDetailesDto
                {
                    QuestionId = q.Id,
                    Title = q.Title,
                    Score = q.Score,
                    LogoUrl = imagesDict.GetValueOrDefault(q.Id) ?? string.Empty,
                    QuestionType = q.QuestionType?.Name ?? string.Empty,
                    QuestionTypeKey = q.QuestionType?.Key ?? 0,
                    Answers = q.QuestionAnswers.Select(a => new QuestionAnswerDetailesDto
                    {
                        AnswerId = a.Id,
                        Answer = a.Answer,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };

            return new ResponseApi<QuizDetailsDto> { Data = dto, Success = true, Message = "Retrieved successfully" };
        }


        public async Task<QuizFullDetailModel> GetFullDetailsAsync(Guid refId, bool isExam)
        {
            if (isExam)
            {
                var exam = await _examRepository.GetAsync(refId);
                if (exam == null) throw new UserFriendlyException("Exam not found");

                var examQuestions = await _examQuestionRepository.GetQuestionsByExamIdAsync(refId);

                return new QuizFullDetailModel
                {
                    Id = exam.Id,
                    Title = exam.Name,
                    QuizTime = exam.TimeExam,
                    TryCount = 0, // أو القيمة الموجودة في جدول الامتحان
                    Questions = examQuestions.Select(x => x.Question).ToList()
                };
            }
            else
            {
                var quizFull = await _quizRepository.GetQuizWithQuestionsAsync(refId);
                if (quizFull == null) throw new UserFriendlyException("Quiz not found");

                return new QuizFullDetailModel
                {
                    Id = quizFull.Id,
                    Title = quizFull.Title,
                    QuizTime = quizFull.QuizTime,
                    TryCount = quizFull.Lecture?.QuizTryCount ?? 0,
                    Questions = quizFull.Questions?.ToList() ?? new List<Question>()
                };
            }
        }

        public async Task<ResponseApi<LectureWithQuizzesDto>> GetLectureWithQuizzesAsync(Guid refId, bool isCourse)
        {
            List<Quiz> quizzes = new();
            string title = "";
            Guid id = Guid.Empty;

            if (!isCourse)
            {
                var lecture = await (await _lectureRepository.GetQueryableAsync())
                    .Include(l => l.Quizzes).ThenInclude(q => q.Questions).ThenInclude(qq => qq.QuestionAnswers)
                    .Include(l => l.Quizzes).ThenInclude(q => q.Questions).ThenInclude(qq => qq.QuestionType)
                    .FirstOrDefaultAsync(l => l.Id == refId);

                if (lecture == null) return new ResponseApi<LectureWithQuizzesDto> { Success = false, Message = "Lecture not found" };

                quizzes = lecture.Quizzes.ToList();
                title = lecture.Title;
                id = lecture.Id;
            }
            else
            {
                var course = await (await _courseRepository.GetQueryableAsync())
                    .Include(c => c.Quizzes).ThenInclude(q => q.Questions).ThenInclude(qq => qq.QuestionAnswers)
                    .Include(c => c.Quizzes).ThenInclude(q => q.Questions).ThenInclude(qq => qq.QuestionType)
                    .FirstOrDefaultAsync(c => c.Id == refId);

                if (course == null) return new ResponseApi<LectureWithQuizzesDto> { Success = false, Message = "Course not found" };

                quizzes = course.Quizzes.ToList();
                title = course.Name; // أو Name حسب موديل الكورس عندك
                id = course.Id;
            }

            // تجهيز روابط الصور مرة واحدة بدلاً من الـ Loop (أداء أفضل بكتير)
            var questionIds = quizzes.SelectMany(q => q.Questions).Select(ques => ques.Id).ToList();
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(questionIds);

            var dto = new LectureWithQuizzesDto
            {
                Id = id,
                Title = title,
                Quizzes = quizzes.Select(q => new QuizWithQuestionsDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Questions = q.Questions.Select(ques => new QuestionWithAnswersDto
                    {
                        Id = ques.Id,
                        Title = ques.Title,
                        Score = ques.Score,
                        QuestionTypeId = ques.QuestionTypeId,
                        QuestionTypeName = ques.QuestionType?.Name ?? "",
                        LogoUrl = mediaItemDic.TryGetValue(ques.Id, out var url) ? url : string.Empty,
                        Answers = ques.QuestionAnswers.Select(ans => new QuestionAnswerPanelDto
                        {
                            Id = ans.Id,
                            Answer = ans.Answer,
                            IsCorrect = ans.IsCorrect
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return new ResponseApi<LectureWithQuizzesDto> { Data = dto, Success = true, Message = "Data loaded successfully" };
        }


        public async Task<ResponseApi<LectureTryDto>> UserTryCount(Guid userId,Guid lecId ,Guid quizId)
        {
            var trys = await _lectureTryRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.LectureId == lecId);
            var lecture = await (await _lectureRepository.GetQueryableAsync()).Include(x=>x.Quizzes).FirstOrDefaultAsync(x=>x.Id == lecId);
            var isSucces = await _lectureTryRepository.AnyAsync(x => x.UserId == userId && x.LectureId == lecId && x.IsSucces == true);
            var quizStudent = await (await _quizStudentRepository.GetQueryableAsync()).FirstOrDefaultAsync(x => x.UserId == userId && x.LectureId == lecId && x.QuizId == quizId);
            var totalScore = await (await _questionRepository.GetQueryableAsync())
                .Where(x => x.QuizId == quizId)
                .SumAsync(x => (double?)x.Score) ?? 0;
            var myScoreRate = lecture?.SuccessQuizRate > 0 && quizStudent != null ? Math.Round(((double)quizStudent.Score / (double)totalScore) * 100, 2) : 0;
            var lecturetry = new LectureTryDto { MyTryCount = trys?.MyTryCount??0, LectureTryCount = lecture?.QuizTryCount??0 * lecture?.Quizzes?.Count??0, IsSucces = isSucces, SuccessQuizRate = lecture?.SuccessQuizRate??0 ,MyScoreRate= myScoreRate };
            return new ResponseApi<LectureTryDto>{ Data = lecturetry, Message="get count" ,Success =true};
        }


        public async Task<Dictionary<Guid, LectureStatusModel>> GetLecturesStatusAsync(Guid userId, List<Guid> lectureIds, List<Guid> quizIds)
        {
            // 1. جلب محاولات المحاضرات دفعة واحدة
            var allTries = await(await _lectureTryRepository.GetQueryableAsync())
                .Where(x => x.UserId == userId && lectureIds.Contains(x.LectureId))
                .ToListAsync();

            // 2. جلب بيانات المحاضرات والكويزات (نحتاج معدل النجاح وعدد المحاولات المتاحة)
            var lectures = await (await _lectureRepository.GetQueryableAsync())
                .Include(x => x.Quizzes)
                .Where(x => lectureIds.Contains(x.Id))
                .ToListAsync();

            // 3. جلب نتائج الكويزات للطلاب
            var quizStudents = await (await _quizStudentRepository.GetQueryableAsync())
                .Where(x => x.UserId == userId && quizIds.Contains(x.QuizId))
                .ToListAsync();

            // 4. جلب مجموع درجات الكويزات (Score) لتجنب حسابها يدوياً
            var quizTotalScores = await (await _questionRepository.GetQueryableAsync())
                .Where(x => quizIds.Contains(x.QuizId))
                .GroupBy(x => x.QuizId)
                .Select(g => new { QuizId = g.Key, TotalScore = g.Sum(x => (double?)x.Score) ?? 0 })
                .ToDictionaryAsync(x => x.QuizId, x => x.TotalScore);

            var result = new Dictionary<Guid, LectureStatusModel>();

            foreach (var lecId in lectureIds)
            {
                var lecture = lectures.FirstOrDefault(l => l.Id == lecId);
                var trys = allTries.FirstOrDefault(t => t.LectureId == lecId);

                // نحدد الكويز المرتبط بهذه المحاضرة من القائمة الممررة
                var currentQuizId = quizIds.FirstOrDefault(qId => lecture?.Quizzes.Any(q => q.Id == qId) ?? false);
                var quizStudent = quizStudents.FirstOrDefault(qs => qs.QuizId == currentQuizId);

                quizTotalScores.TryGetValue(currentQuizId, out var totalScore);

                var myScoreRate = (lecture?.SuccessQuizRate > 0 && quizStudent != null && totalScore > 0)
                    ? Math.Round((quizStudent.Score / totalScore) * 100, 2)
                    : 0;

                result[lecId] = new LectureStatusModel
                {
                    MyTryCount = trys?.MyTryCount ?? 0,
                    // المعادلة: عدد محاولات المحاضرة * عدد الكويزات
                    LectureTryCount = (lecture?.QuizTryCount ?? 0) * (lecture?.Quizzes?.Count ?? 0),
                    IsSucces = trys?.IsSucces ?? false,
                    SuccessQuizRate = lecture?.SuccessQuizRate ?? 0,
                    MyScoreRate = myScoreRate
                };
            }

            return result;
        }

    }
}
