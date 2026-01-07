using AutoMapper;
using Dev.Acadmy.Chapters;
using Dev.Acadmy.Courses;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Enums;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.LookUp;
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
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Entities.Courses.Managers
{
    public class CourseManager : DomainService
    {
        private readonly IRepository<Entities.Course> _courseRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly MediaItemManager _mediaItemManager; 
        private readonly IIdentityUserRepository _userRepository;
        private readonly QuestionBankManager _questionBankManager;
        private readonly ChapterManager _chapterManager;
        private readonly CourseInfoManager _courseInfoManager;
        private readonly LectureManager _lectureManager;
        private readonly QuizManager _quizManger;
        private readonly QuestionManager _questionManager;
        private readonly IRepository<QuizStudent, Guid> _quizStudentRepository;
        private readonly IRepository<Chapter, Guid> _chapterRepository;
        private readonly IRepository<LectureTry, Guid> _lectureTryRepository;
        private readonly IRepository<Lecture, Guid> _lectureRepository;
        private readonly ExamManager _examManager;
        private readonly IRepository<Exam ,Guid> _examRepository;
        private readonly IRepository<QuestionBank, Guid> _questionBankRepository;
        private readonly ICourseRepository _courseRepo;
        private readonly ICourseFeedbackRepository _courseFeedbackRepo;
        private readonly IMediaItemRepository _mediaItemRepo;
        private readonly IQuizRepository _quizRepo;
        private readonly ICourseStudentRepository _courseStudentRepository;
        public CourseManager(IQuizRepository quizRepo, IMediaItemRepository mediaItemRepo, ICourseFeedbackRepository courseFeedbackRepo,ICourseRepository courseRepo,  IRepository<QuestionBank, Guid> questionBankRepository, IRepository<Exam ,Guid> examRepository, ExamManager examManager, IRepository<Lecture, Guid> lectureRepository, IRepository<LectureTry, Guid> lectureTryRepository, IRepository<Chapter, Guid> chapterRepository,IRepository<QuizStudent, Guid> quizStudentRepository, QuestionManager questionManager, QuizManager quizManger, LectureManager lectureManager, CourseInfoManager courseInfoManager, ChapterManager chapterManager, ICourseStudentRepository courseStudentRepository, QuestionBankManager questionBankManager, IIdentityUserRepository userRepository, MediaItemManager mediaItemManager, ICurrentUser currentUser, IRepository<Entities.Course> courseRepository , IMapper mapper) 
        {
            _quizRepo = quizRepo;
            _mediaItemRepo = mediaItemRepo;
            _courseFeedbackRepo = courseFeedbackRepo;
            _courseRepo = courseRepo;
            _questionBankRepository = questionBankRepository;
            _examRepository = examRepository;
            _examManager = examManager;
            _lectureRepository = lectureRepository;
            _lectureTryRepository = lectureTryRepository;
            _chapterRepository = chapterRepository;
            _quizStudentRepository = quizStudentRepository;
            _questionManager = questionManager;
            _quizManger = quizManger;
            _lectureManager = lectureManager;
            _courseInfoManager = courseInfoManager;
            _chapterManager = chapterManager;
            _courseStudentRepository = courseStudentRepository;
            _questionBankManager = questionBankManager;
            _userRepository = userRepository;
            _mediaItemManager = mediaItemManager;
            _currentUser = currentUser;
            _mapper = mapper;
            _courseRepository = courseRepository;
        }

        public async Task<ResponseApi<CourseDto>> GetAsync(Guid id)
        {
            var course = await(await _courseRepository.GetQueryableAsync()).Include(x=>x.CourseInfos).FirstOrDefaultAsync(x => x.Id == id);
            if (course == null) return new ResponseApi<CourseDto> { Data = null, Success = false, Message = "Not found Course" };
            var dto = _mapper.Map<CourseDto>(course);
            var mediaItem = await _mediaItemManager.GetAsync(dto.Id );
            dto.LogoUrl = mediaItem?.Url ?? "";
            foreach(var info in course.CourseInfos) dto.Infos.Add(info.Name);
            
            return new ResponseApi<CourseDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<(List<Entities.Course> Items, long TotalCount)> GetListAsync(int skip, int take, string? search, CourseType type, Guid? userId, bool isAdmin)=> await _courseRepo.GetListWithDetailsAsync(skip, take, search, type, userId, isAdmin);
        public async Task<Entities.Course> CreateWithDependenciesAsync(Entities.Course course)
        {
            var result = await _courseRepository.InsertAsync(course);
            if (result.IsQuiz) await _quizRepo.InsertAsync(new Quiz { CourseId = result.Id, Title = result.Title, Description = result.Description ,QuizTryCount=0 , QuizTime = 0 });
            return result;
        }

        public async Task<Entities.Course> UpdateWithDependenciesAsync(Entities.Course course)
        {
            var result = await _courseRepository.UpdateAsync(course);
            if (result.IsQuiz && !await _quizRepo.AnyAsync(x => x.CourseId == result.Id)) await _quizRepo.InsertAsync(new Quiz { CourseId = result.Id, Title = result.Title, Description = result.Description, QuizTryCount = 0, QuizTime = 0 });
            return result;
        }
        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());
            var course = await (await _courseRepository.GetQueryableAsync()).Include(x=>x.Chapters).Include(x=>x.CourseInfos).FirstOrDefaultAsync(x => x.Id == id);
            if (course == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found Course" };
            var courseStudent =await (await _courseStudentRepository.GetQueryableAsync()).Where(x=>x.CourseId == course.Id).ToListAsync();
            if (courseStudent != null) await _courseStudentRepository.DeleteManyAsync(courseStudent);
           // var questionBank = await _questionBankManager.GetByCourse(id);
           // if (questionBank != null) await _questionBankManager.DeleteAsync(questionBank.Id);
            var chapterIds = course.Chapters.Select(x => x.Id).ToList();
            if (chapterIds.Any()) foreach (var chapter in chapterIds) await _chapterManager.DeleteAsync(chapter);
            var infos = course.CourseInfos.Select(x => x.Id);
            if (infos.Any()) foreach (var info in infos) await _courseInfoManager.DeleteAsync(info);
            await _mediaItemManager.DeleteAsync(id);
            await _courseRepository.DeleteAsync(course);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetCoursesListAsync()
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());

            var queryable = await _courseRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var courses = new List<Entities.Course>();
            if(roles.Any(x=>x.Name.ToUpper()==RoleConsts.Admin)) courses = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime));
            else courses = await AsyncExecuter.ToListAsync(queryable.Where(c => c.UserId == _currentUser.GetId()).OrderByDescending(c => c.CreationTime));
            var courseDtos = _mapper.Map<List<LookupDto>>(courses);
            return new PagedResultDto<LookupDto>(totalCount, courseDtos);
        }

        public async Task<PagedResultDto<CourseInfoHomeDto>> GetCoursesInfoListAsync(
            int pageNumber,
            int pageSize,
            string? search,
            bool alreadyJoin,
            Guid collegeId,
            Guid? subjectId,
            Guid? gradelevelId,
            Guid? termId)
        {
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());

            if (collegeId == Guid.Empty)
                return new PagedResultDto<CourseInfoHomeDto>(0, new List<CourseInfoHomeDto>());

            var courseStudents = await (await _courseStudentRepository.GetQueryableAsync())
                .Where(x => x.UserId == currentUser.Id)
                .ToListAsync();

            var alreadyJoinCourses = courseStudents
                .Where(x => x.IsSubscibe)
                .Select(x => x.CourseId)
                .ToList();

            var alreadyRequestCourses = courseStudents
                .Select(x => x.CourseId)
                .ToList();

            var queryable = await _courseRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryable = queryable
                    .Include(c => c.Subject)
                        .ThenInclude(x => x.GradeLevel)
                    .Where(c =>
                        c.Name.Contains(search) ||
                        c.Description.Contains(search) ||
                        c.Subject.Name.Contains(search));
            }

            // ✅ تعديل هنا
            if (alreadyJoin)
            {
                // نجيب فقط الكورسات اللي الطالب مشترك فيها في أي كلية
                queryable = queryable.Where(c => alreadyJoinCourses.Contains(c.Id));
            }
            else
            {
                // نحافظ على الفلاتر العادية حسب الكلية والمادة والمستوى
                queryable = queryable.Include(x=>x.Subject).Where(c =>
                    c.CollegeId == collegeId &&
                    (!subjectId.HasValue || c.SubjectId == subjectId.Value) &&
                    (!termId.HasValue || c.Subject.TermId == termId.Value) &&
                    (!gradelevelId.HasValue || c.Subject.GradeLevelId == gradelevelId.Value));
            }

            var totalCount = await queryable.CountAsync();

            var courses = await queryable
                .Include(c => c.User)
                .Include(c => c.College)
                .Include(c => c.Chapters)
                .OrderByDescending(c => c.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mediaItems = new Dictionary<Guid, MediaItem>();
            var courseIds = courses.Select(x => x.Id).ToList();
            var coursesCountDic = await _courseStudentRepository.GetTotalSubscribersPerCourseAsync(courseIds);
            // ✅ تحويل البيانات لقائمة حقيقية قبل إرسالها للميثود
            var mediaItemDic = await _mediaItemRepo.GetUrlDictionaryByRefIdsAsync(courseIds); 
            var courseDtos = courses.Select(course => new CourseInfoHomeDto
            {
                Id = course.Id,
                Name = course.Name, 
                IsPdf = course.IsPdf, 
                PdfUrl = course.PdfUrl,
                Description = course.Description,
                Price = course.Price,
                LogoUrl = mediaItemDic.TryGetValue(course.Id, out var media)? media: "",
                SubscriberCount = coursesCountDic.TryGetValue(course.Id, out var subcount) ? subcount : 0,
                UserId = course.UserId,
                UserName = course.User?.Name ?? "",
                CollegeId = course.CollegeId,
                CollegeName = course.College?.Name ?? "",
                AlreadyJoin = alreadyJoinCourses.Contains(course.Id),
                AlreadyRequest = alreadyRequestCourses.Contains(course.Id),
                SubjectId = course.Subject?.Id,
                SubjectName = course.Subject?.Name ?? "",
                ChapterCount = course.Chapters.Count,
                DurationInWeeks = course.DurationInDays / 7,
                GradelevelId = course.Subject?.GradeLevelId ?? null,
                GradelevelName = course.Subject?.GradeLevel?.Name ?? string.Empty,
                YouTubeVideoUrl = course.YouTubeVideoUrl?? string.Empty,
                DriveVideoUrl = course.DriveVideoUrl?? string.Empty,
                HasYouTubeVideo=course.HasYouTubeVideo,
                HasDriveVideo=course.HasDriveVideo, 
                IsQuiz = course.IsQuiz,
                ShowSubscriberCount = course.ShowSubscriberCount,
            }).ToList();

            return new PagedResultDto<CourseInfoHomeDto>(totalCount, courseDtos);
        }
        // home
        public async Task<ResponseApi<CourseInfoHomeDto>> GetCoursesInfoAsync(Guid courseId)
        {
            var currentUserId = _currentUser.GetId();

            // 1. جلب بيانات الكورس الأساسية (باستخدام الريبو المخصص)
            var course = await _courseRepo.GetWithHomeDetailesAsync(courseId);
            if (course == null)
                throw new UserFriendlyException("Course Not Found");

            // 2. التحقق من حالة المستخدم (هل انضم أو طلب الانضمام؟)
            var studentStatus = await (await _courseStudentRepository.GetQueryableAsync())
                .Where(x => x.CourseId == courseId && x.UserId == currentUserId)
                .Select(x => new { x.IsSubscibe, x.CourseId })
                .FirstOrDefaultAsync();

            // 3. جلب الـ Feedbacks (آخر 5 تقييمات مقبولة)
            var feedbacks = await _courseFeedbackRepo.GetListSumFeedByCourseIdAsync(courseId, 5);

            // 4. جلب صور المستخدمين أصحاب التقييمات (استخدام Dictionary للأداء)
            var userIds = feedbacks.Select(x => x.UserId).Distinct().ToList();
            var mediaItemDic = await _mediaItemRepo.GetUrlDictionaryByRefIdsAsync(userIds);

            // دمج الصور داخل الـ Feedbacks
            foreach (var fb in feedbacks)
            {
                mediaItemDic.TryGetValue(fb.UserId, out var userImg);
                fb.LogoUrl = userImg ?? "";
            }

            // 5. جلب الميديا الخاصة بالكورس والمدرس والشباتر
            var media = await _mediaItemManager.GetAsync(courseId);
            var mediaUser = await _mediaItemManager.GetAsync(course.UserId);
            var chapters = await GetCourseChaptersListAsync(courseId, true);

            // 6. حساب الإحصائيات (عدد المحاضرات، متوسط التقييم)
            int totalLectureCount = chapters.Sum(ch => ch.Lectures.Count);
            // حساب متوسط التقييم من الـ Feedbacks الموجودة في الكيان
            double averageRating = course.Feedbacks.Any() ? course.Feedbacks.Average(x => x.Rating) : 0;

            // 7. بناء الـ DTO النهائي
            var courseDto = new CourseInfoHomeDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Price = course.Price,
                LogoUrl = media?.Url ?? "",
                UserId = course.UserId,
                UserName = course.User?.Name ?? "",
                TeacherLogoUrl = mediaUser?.Url ?? "",
                IsQuiz= course.IsQuiz,
                IsPdf = course.IsPdf,
                PdfUrl = course.PdfUrl,
                LectureCount = totalLectureCount,
                ChapterCount = course.Chapters.Count,
                DurationInWeeks = course.DurationInDays / 7,
                YouTubeVideoUrl = course.YouTubeVideoUrl,
                DriveVideoUrl = course.DriveVideoUrl,
                HasDriveVideo = course.HasDriveVideo,
                HasYouTubeVideo = course.HasYouTubeVideo,
                CollegeId = course.CollegeId,
                CollegeName = course.College?.Name ?? "",
                SubjectId = course.Subject?.Id,
                SubjectName = course.Subject?.Name ?? "",
                ShowSubscriberCount = course.ShowSubscriberCount,
                // التقييمات وحالة المستخدم
                Rating = (float)Math.Round(averageRating, 1),
                Feedbacks = feedbacks, // القائمة التي جلبناها مع صورها
                AlreadyJoin = studentStatus?.IsSubscibe ?? false,
                AlreadyRequest = studentStatus != null,

                Infos = course.CourseInfos.Select(x => x.Name).ToList(),
                courseChaptersDtos = chapters
            };

            return new ResponseApi<CourseInfoHomeDto>
            {
                Data = courseDto,
                Success = true,
                Message = "Find Course Success"
            };
        }


        public async Task<List<CourseChaptersDto>> GetCourseChaptersListAsync(Guid courseId, bool IsFree)
        {
            var userId = _currentUser.GetId();

            // ✅ الكويزات التي أجابها المستخدم مسبقاً
            var answeredLectureIds = await (await _quizStudentRepository.GetQueryableAsync())
                .Where(qs => qs.UserId == userId)
                .Select(qs => qs.LectureId)
                .ToListAsync();

            // ✅ كل محاولات المستخدم على المحاضرات
            var lectureTries = await (await _lectureTryRepository.GetQueryableAsync())
                .Where(lt => lt.UserId == userId)
                .ToListAsync();

            var queryable = await _chapterRepository.GetQueryableAsync();
            var query = queryable
                .Include(x => x.Course)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quizzes)
                        .ThenInclude(q => q.Questions)
                .Where(c => c.CourseId == courseId);

            // ✅ لو المستخدم طالب فقط الدروس أو الفصول المجانية
            if (IsFree)
            {
                query = query.Where(c => c.IsFree || c.Lectures.Any(l => l.IsFree));
            }

            var chapters = await query
                .OrderBy(c => c.CreationTime)
                .ToListAsync();

            var chapterInfoDtos = new List<CourseChaptersDto>();

            foreach (var c in chapters)
            {
                var lectureDtos = new List<LectureInfoDto>();

                // ✅ لو IsFree true → نعرض فقط المحاضرات المجانية داخل الشابتر
                var lecturesQuery = c.Lectures.Where(x => x.IsVisible);
                if (IsFree)
                    lecturesQuery = lecturesQuery.Where(l => l.IsFree);

                foreach (var l in lecturesQuery)
                {
                    var media = await _mediaItemManager.GetAsync(l.Id);

                    var lectureTry = lectureTries.FirstOrDefault(x => x.LectureId == l.Id)
                        ?? new LectureTry { LectureId = l.Id, UserId = userId, MyTryCount = 0 };

                    int maxAttempts = l.Quizzes.Count > 0 ? l.Quizzes.Count : 1;
                    var quizzes = l.Quizzes.OrderBy(q => q.CreationTime).ToList();

                    QuizInfoDto quizDto;

                    if (quizzes.Any())
                    {
                        int index = lectureTry.MyTryCount;
                        if (index >= quizzes.Count || lectureTry.MyTryCount >= maxAttempts)
                            index = quizzes.Count - 1;

                        var nextQuiz = quizzes[index];

                        quizDto = new QuizInfoDto
                        {
                            QuizId = nextQuiz.Id,
                            Title = nextQuiz.Title,
                            QuestionsCount = nextQuiz.Questions.Count,
                            QuizTryCount = l.QuizTryCount,
                            TryedCount = lectureTry.MyTryCount,
                            AlreadyAnswer = answeredLectureIds.Contains(l.Id)
                        };
                    }
                    else
                    {
                        quizDto = new QuizInfoDto
                        {
                            QuizId = Guid.Empty,
                            Title = "لا يوجد كويز متاح",
                            QuestionsCount = 0,
                            QuizTryCount = 0,
                            TryedCount = 0,
                            AlreadyAnswer = false
                        };
                    }

                    var lectureDto = new LectureInfoDto
                    {
                        LectureId = l.Id,
                        Title = l.Title,
                        Content = l.Content,
                        YouTubeVideoUrl = l.YouTubeVideoUrl,
                        DriveVideoUrl = l.DriveVideoUrl,
                        Quiz = quizDto
                    };

                    var lecPdfs = await _mediaItemManager.GetListAsync(l.Id);
                    foreach (var pdf in lecPdfs)
                        if (!pdf.IsImage)
                            lectureDto.PdfUrls.Add(pdf.Url);

                    lectureDtos.Add(lectureDto);
                }

                var creatorCourse = await _userRepository.GetAsync(c.Course.UserId);
                var mediaItemUser = await _mediaItemManager.GetAsync(creatorCourse.Id);

                chapterInfoDtos.Add(new CourseChaptersDto
                {
                    CourseId = c.CourseId,
                    CourseName = c.Course.Name,
                    ChapterId = c.Id,
                    ChapterName = c.Name,
                    UserId = creatorCourse.Id,
                    UserName = creatorCourse.Name,
                    LogoUrl = mediaItemUser?.Url ?? string.Empty,
                    LectureCount = lectureDtos.Count,
                    Lectures = lectureDtos
                });
            }

            return chapterInfoDtos;
        }


        public async Task<PagedResultDto<LookupDto>> GetMyCoursesLookUpAsync()
        {
            // هات الـ User الحالي
            var currentUserId = _currentUser.GetId();

            // هات كل الكورسات اللي انت الـ CreatorId فيها
            var queryable = await _courseRepository.GetQueryableAsync();
            var myCourses = await queryable
                .Where(c => c.UserId == currentUserId)
                .ToListAsync();
            if (!myCourses.Any()) return new PagedResultDto<LookupDto>(0,new List<LookupDto>() );
            // اعمل Map للـ DTO
            var courseDtos = _mapper.Map<List<LookupDto>>(myCourses);
            return new PagedResultDto<LookupDto>( courseDtos.Count(),courseDtos);
        }

        public async Task<Guid> DuplicateCourseAsync(Guid courseId)
        {
            // 🟢 1. تحميل الكورس الأصلي بكامل العلاقات
            var course = await (await _courseRepository.GetQueryableAsync())
                .Include(x => x.Chapters)
                    .ThenInclude(ch => ch.Lectures)
                        .ThenInclude(l => l.Quizzes)
                            .ThenInclude(q => q.Questions)
                                .ThenInclude(qq => qq.QuestionAnswers)
                            .Include(x=>x.CourseInfos)
                .FirstOrDefaultAsync(x => x.Id == courseId);

            if (course == null)
                throw new UserFriendlyException("Course not found");

            // 🟢 2. إنشاء نسخة جديدة من الكورس
            var newCourse = new Entities.Course
            {
                Name = course.Name + " (Copy)",
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                Rating = 0,
                UserId = _currentUser.GetId(),
                CollegeId = course.CollegeId,
                SubjectId = course.SubjectId,
                IsActive = true,
                IsLifetime = course.IsLifetime,
                DurationInDays = course.DurationInDays,
                IsPdf = course.IsPdf,
                PdfUrl= course.PdfUrl,
                YouTubeVideoUrl= course.YouTubeVideoUrl,
                DriveVideoUrl = course.DriveVideoUrl,

            };
            var resultCourse = await _courseRepository.InsertAsync(newCourse, autoSave: true);
            await _mediaItemManager.CreateAsync(new CreateUpdateMediaItemDto { Url =  _mediaItemManager.GetAsync(course.Id).Result?.Url ?? "", RefId = resultCourse.Id, IsImage = true });
            //var bankStatic = await _questionBankManager.CreateAsync(new CreateUpdateQuestionBankDto { Name = resultCourse.Name + "Question Bank", CourseId = resultCourse.Id });

            // await _questionBankManager.CreateAsync(new CreateUpdateQuestionBankDto { CreatorId = newCourse.UserId, CourseId = newCourse.Id, Name = $"{newCourse.Name} Question Bank (Copy)" });
            // انسخ CourseInfos
            foreach (var info in course.CourseInfos)
            {
                await _courseInfoManager.CreateAsync(new CreateUpdateCourseInfoDto { Name = info.Name, CourseId = resultCourse.Id });
            }


            // 🟢 3. نسخ الفصول (Chapters)
            foreach (var chapter in course.Chapters)
            {
                var newChapter = new CreateUpdateChapterDto
                {
                    Name = chapter.Name,
                    CourseId = resultCourse.Id
                };
                var chapterDto = await _chapterManager.CreateAsync(newChapter);

                // 🟢 4. نسخ المحاضرات (Lectures)
                foreach (var lecture in chapter.Lectures)
                {
                    var newLecture = new CreateUpdateLectureDto
                    {
                        Title = lecture.Title,
                        Content = lecture.Content,
                        YouTubeVideoUrl = lecture.YouTubeVideoUrl,
                        DriveVideoUrl = lecture.DriveVideoUrl,
                        ChapterId = chapterDto.Data.Id,
                        IsVisible = lecture.IsVisible,
                        QuizTryCount = lecture.QuizTryCount,
                        IsFree = lecture.IsFree,
                        QuizCount=lecture.Quizzes.Count(),
                        QuizTime=lecture.Quizzes.First().QuizTime,
                        SuccessQuizRate=lecture.SuccessQuizRate,
                    };
                    var lecPdfs =( await _mediaItemManager.GetListAsync(lecture.Id)).Select(x=>x.Url).ToList();
                    newLecture.PdfUrls = lecPdfs;
                    var lecDto  = await _lectureManager.CreateAsync(newLecture);

                    // 🟢 5. نسخ الكويزات (Quizzes)
                    foreach (var quiz in lecture.Quizzes)
                    {
                        var newQuiz = new CreateUpdateQuizDto
                        {
                            Title = quiz.Title,
                            Description = quiz.Description,
                            QuizTime = quiz.QuizTime,
                            QuizTryCount = quiz.QuizTryCount,
                            LectureId = lecDto.Data.Id
                        };
                        var quizDto = await _quizManger.CreateAsync(newQuiz);
                        // 🟢 6. نسخ الأسئلة (Questions)
                        foreach (var question in quiz.Questions)
                        {
                            
                            var newQuestion = new CreateUpdateQuestionDto
                            {
                                Title = question.Title,
                                QuizId = quizDto.Data.Id,
                                QuestionTypeId = question.QuestionTypeId,
                                QuestionBankId =null,
                                Answers = question.QuestionAnswers.Select(a => new CreateUpdateQuestionAnswerDto
                                {
                                    Answer = a.Answer,
                                    
                                    IsCorrect = a.IsCorrect,
                                }).ToList(),
                                
                                Score=question.Score
                            };
                            var questionMediaItem = await _mediaItemManager.GetAsync(question.Id);
                            newQuestion.LogoUrl = questionMediaItem?.Url ?? "";
                            await _questionManager.CreateAsync(newQuestion);
                        }
                    }
                }
            }
            return newCourse.Id;
        }


        public async Task<PagedResultDto<LectureWithQuizzesDto>> GetStudentQuizzesByCourseAsync(Guid courseId, Guid userId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            // 1. جلب سجلات تسليم الطالب للكويزات مع إجاباته التفصيلية
            var studentSubmissions = await (await _quizStudentRepository.GetQueryableAsync())
                .Include(qs => qs.Answers) // جلب جدول QuizStudentAnswer
                .Where(qs => qs.UserId == userId)
                .ToListAsync();

            var answeredQuizIds = studentSubmissions.Select(qs => qs.QuizId).ToList();

            // 2. جلب المحاضرات التي تحتوي على هذه الكويزات فقط
            var query = await _lectureRepository.GetQueryableAsync();

            var filteredLecturesQuery = query
                .Include(l => l.Chapter)
                .Include(l => l.Quizzes).ThenInclude(q => q.Questions).ThenInclude(qq => qq.QuestionAnswers)
                .Include(l => l.Quizzes).ThenInclude(q => q.Questions).ThenInclude(qq => qq.QuestionType)
                .Where(l => l.Chapter.CourseId == courseId && l.Quizzes.Any(q => answeredQuizIds.Contains(q.Id)));

            var totalCount = await filteredLecturesQuery.CountAsync();

            var pagedLectures = await filteredLecturesQuery
                .OrderByDescending(l => l.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 3. عملية الـ Mapping ودمج البيانات
            var resultDtos = pagedLectures.Select(lecture => new LectureWithQuizzesDto
            {
                Id = lecture.Id,
                Title = lecture.Title,
                Quizzes = lecture.Quizzes
                    .Where(q => answeredQuizIds.Contains(q.Id)) // عرض الكويزات المحلولة فقط
                    .Select(q =>
                    {
                        // البحث عن محاولة الطالب لهذا الكويز
                        var submission = studentSubmissions.FirstOrDefault(s => s.QuizId == q.Id);

                        return new QuizWithQuestionsDto
                        {
                            Id = q.Id,
                            Title = q.Title,
                            Questions = q.Questions.Select(ques =>
                            {
                                // البحث عن إجابة الطالب على هذا السؤال تحديداً
                                var studentResponse = submission?.Answers.FirstOrDefault(a => a.QuestionId == ques.Id);

                                return new QuestionWithAnswersDto
                                {
                                    Id = ques.Id,
                                    Title = ques.Title,
                                    Score = ques.Score,
                                    QuestionTypeId = ques.QuestionTypeId,
                                    QuestionTypeName = ques.QuestionType?.Name ?? "",

                                    // بيانات إجابة الطالب
                                    StudentTextAnswer = studentResponse?.TextAnswer?? string.Empty,
                                    IsStudentAnswerCorrect = studentResponse?.IsCorrect ?? false,
                                    ScoreObtained = studentResponse?.ScoreObtained ?? 0,

                                    // دمج خيارات السؤال مع تحديد ما اختاره الطالب (للأسئلة الاختيارية)
                                    Answers = ques.QuestionAnswers.Select(ans => new QuestionAnswerPanelDto
                                    {
                                        Id = ans.Id,
                                        Answer = ans.Answer,
                                        IsCorrect = ans.IsCorrect, // الحل النموذجي
                                        IsSelectedByStudent = studentResponse?.SelectedAnswerId == ans.Id // هل هذا هو خيار الطالب؟
                                    }).ToList()
                                };
                            }).ToList()
                        };
                    }).ToList()
            }).ToList();

            return new PagedResultDto<LectureWithQuizzesDto>(totalCount, resultDtos);
        }

    }
}
