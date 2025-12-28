using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Microsoft.EntityFrameworkCore;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Response;
using Dev.Acadmy.Questions;
using Volo.Abp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Interfaces;
namespace Dev.Acadmy.Lectures
{
    public class LectureManager:DomainService
    {
        private readonly IRepository<Lecture,Guid> _lectureRepository;
        private readonly IMapper _mapper;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly QuizManager _quizManager;
        private readonly IRepository<Quiz ,Guid> _quizRepository;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IRepository<LectureStudent ,Guid> _lectureStudentRepository;
        private readonly IRepository<LectureTry , Guid> _lectureTryRepository;
        private readonly IRepository<QuizStudent, Guid> _quizStudentRepository;
        private readonly IRepository<Question, Guid> _questionRepository;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly ICourseRepository _courseRepository;
        public LectureManager(ICourseRepository courseRepository, IMediaItemRepository mediaItemRepository, IRepository<Question, Guid> questionRepository, IRepository<QuizStudent, Guid> quizStudentRepository, IRepository<LectureTry, Guid> lectureTryRepository, IRepository<LectureStudent, Guid> lectureStudentRepository, MediaItemManager mediaItemManager, IRepository<Quiz,Guid> quizRepository, QuizManager quizManager, ICurrentUser currentUser, IIdentityUserRepository userRepository, IMapper mapper, IRepository<Lecture,Guid> lectureRepository)
        {
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
                ChapterName = l.Chapter?.Name,
                Content = l.Content,
                Title = l.Title,
                YouTubeVideoUrl = l.YouTubeVideoUrl,
                DriveVideoUrl = l.DriveVideoUrl,
                HasYouTubeVideo = l.HasYouTubeVideo,
                HasDriveVideo = l.HasDriveVideo,
                CourseId = l.Chapter?.CourseId ?? Guid.Empty,
                CourseName = l.Chapter?.Course?.Name,
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
            for (var i = 0; i < input.QuizCount; i++) await _quizManager.CreateAsync(new CreateUpdateQuizDto {QuizTryCount=input.QuizTryCount, CreaterId = _currentUser.GetId(), QuizTime = input.QuizTime, LectureId = lectureId,Title = input.Title+" " + "Quiz " + i, Description = input.Content });
        }
        // here
        public async Task<ResponseApi<QuizDetailsDto>> GetQuizDetailsAsync(Guid quizId)
        {
         
            var queryable = await _quizRepository.GetQueryableAsync();
            var quiz = await queryable
                .Include(x=>x.Lecture)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.QuestionAnswers)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.QuestionType)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) throw new UserFriendlyException("Quiz not found");
            //var tryCount = await _lectureTryRepository.FirstOrDefaultAsync(x=>x.LectureId ==(Guid) quiz.LectureId && x.UserId ==_currentUser.GetId());
            //if(tryCount == null) tryCount =  await _lectureTryRepository.InsertAsync(new LectureTry { LectureId =(Guid) quiz.LectureId, UserId = _currentUser.GetId(), MyTryCount = 1 },autoSave:true);
            //else
            //{
            //    if (tryCount.MyTryCount >= quiz.Lecture.QuizTryCount) throw new UserFriendlyException("You have reached the maximum number of attempts for this quiz.");
            //    tryCount.MyTryCount += 1;
            //    await _lectureTryRepository.UpdateAsync(tryCount ,autoSave:true);
            //}
            var dto = new QuizDetailsDto
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                QuizTime = quiz.QuizTime,
                QuizTryCount = quiz?.Lecture?.QuizTryCount ?? 0,
                Questions = quiz.Questions.Select(q => new QuestionDetailesDto
                {
                    QuestionId = q.Id,
                    Title = q.Title,
                    Score = q.Score,
                    LogoUrl =_mediaItemManager.GetAsync(q.Id).Result?.Url ?? string.Empty,
                    QuestionType = q.QuestionType?.Name ?? "",
                    QuestionTypeKey = q.QuestionType?.Key ?? 0, // إضافة الـ Key

                    Answers = q.QuestionAnswers.Select(a => new QuestionAnswerDetailesDto
                    {
                        AnswerId = a.Id,
                        Answer = a.Answer,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };
            return new ResponseApi <QuizDetailsDto> { Data = dto, Success = true, Message = "find success" };
        }

        public async Task<ResponseApi<LectureWithQuizzesDto>> GetLectureWithQuizzesAsync(Guid refId ,bool isCourse)
        {
            var course = new Entities.Courses.Entities.Course();
            var lecture = new Lecture();
            if (!isCourse)
            lecture =await (await _lectureRepository.GetQueryableAsync())
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qq => qq.QuestionAnswers)
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qq => qq.QuestionType) // 👈 عشان نجيب اسم النوع
                .FirstOrDefaultAsync(l => l.Id == refId);

            else
            course = await (await _courseRepository.GetQueryableAsync())
                .Include(c => c.Quizzes)
                    .ThenInclude(c =>c.Questions)
                        .ThenInclude(qq => qq.QuestionAnswers)
                .Include(l => l.Quizzes)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qq => qq.QuestionType) // 👈 عشان نجيب اسم النوع
                .FirstOrDefaultAsync(c => c.Id == refId);

            if (lecture == null)
            {
                return new ResponseApi<LectureWithQuizzesDto>
                {
                    Data = null,
                    Success = false,
                    Message = "Lecture not found"
                };
            }

            var dto = new LectureWithQuizzesDto
            {
                Id = lecture.Id,
                Title = lecture.Title,
                Quizzes = lecture.Quizzes.Select(q => new QuizWithQuestionsDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Questions = q.Questions.Select(ques => new QuestionWithAnswersDto
                    {
                        Id = ques.Id,
                        Title = ques.Title,
                        Score = ques.Score,
                        QuestionTypeId = ques.QuestionTypeId,           
                        QuestionTypeName = ques.QuestionType?.Name?? "",
                        LogoUrl = _mediaItemManager.GetAsync(ques.Id).Result?.Url ?? string.Empty,
                        Answers = ques.QuestionAnswers.Select(ans => new QuestionAnswerPanelDto
                        {
                            Id = ans.Id,
                            Answer = ans.Answer,
                            IsCorrect = ans.IsCorrect
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return new ResponseApi<LectureWithQuizzesDto>
            {
                Data = dto,
                Success = true,
                Message = "Lecture loaded successfully"
            };
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

    }
}
