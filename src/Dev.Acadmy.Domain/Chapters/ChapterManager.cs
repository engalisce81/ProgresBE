using AutoMapper;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Response;
using Microsoft.EntityFrameworkCore;
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

namespace Dev.Acadmy.Chapters
{
    public class ChapterManager:DomainService
    {
        private readonly IRepository<Chapter> _chapterRepository;
        private readonly LectureManager _lectureManger;
        private readonly IMapper _mapper;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<QuizStudent, Guid> _quizStudentRepository;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IRepository<LectureStudent ,Guid> _lectureStudentRepository;
        private readonly IRepository<LectureTry, Guid> _lectureTryRepository;
        private readonly IRepository<Entities.Courses.Entities.Course , Guid> _courseRepository;
        private readonly IdentityUserManager _userManager;
        public ChapterManager(IdentityUserManager userManager, IRepository<Entities.Courses.Entities.Course, Guid> courseRepository, IRepository<LectureTry, Guid> lectureTryRepository, LectureManager lectureManger, IRepository<LectureStudent, Guid> lectureStudentRepository, MediaItemManager mediaItemManager, IRepository<QuizStudent, Guid> quizStudentRepository, ICurrentUser currentUser, IIdentityUserRepository userRepository, IMapper mapper, IRepository<Chapter> chapterRepository)
        {
            _userManager = userManager;
            _courseRepository = courseRepository;
            _lectureTryRepository = lectureTryRepository;
            _lectureManger = lectureManger;
            _lectureStudentRepository = lectureStudentRepository;
            _mediaItemManager = mediaItemManager;
            _quizStudentRepository = quizStudentRepository;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _chapterRepository = chapterRepository;
            _mapper = mapper;
        }

        public async Task<ResponseApi<ChapterDto>> GetAsync(Guid id)
        {
            var chapter = await _chapterRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (chapter == null) return new ResponseApi<ChapterDto> { Data = null, Success = false, Message = "Not found chapter" };
            var dto = _mapper.Map<ChapterDto>(chapter);
            return new ResponseApi<ChapterDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<ChapterDto>> GetListAsync(int pageNumber, int pageSize, string? search, Guid courseId)
        {
            var skipCount = (pageNumber - 1) * pageSize;
            var currentUserId = _currentUser.GetId();

            // 1. إنشاء الاستعلام الأساسي مع Include و فلترة الكورس الإلزامية
            var queryable = (await _chapterRepository.GetQueryableAsync())
                .Include(x => x.Course)
                .Where(x => x.CourseId == courseId); // استخدام الـ courseId الممرر

            // 2. فلترة البحث (إذا وُجد)
            if (!string.IsNullOrWhiteSpace(search))
            {
                queryable = queryable.Where(c => c.Name.Contains(search) || c.Course.Name.Contains(search));
            }
            var user = await _userRepository.GetAsync(currentUserId);

            // 3. منطق الصلاحيات: إذا لم يكن أدمن، يرى فقط ما أنشأه
            var isAdmin = await _userManager.IsInRoleAsync( user,RoleConsts.Admin.ToLower());
            if (!isAdmin)
            {
                queryable = queryable.Where(c => c.CreatorId == currentUserId);
            }

            // 4. حساب العدد الإجمالي للنتائج المفلترة
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // 5. جلب البيانات بطلب واحد مرتب ومقسم لصفحات
            var chapters = await AsyncExecuter.ToListAsync(
                queryable.OrderByDescending(c => c.CreationTime)
                         .Skip(skipCount)
                         .Take(pageSize)
            );

            // 6. التحويل لـ DTO
            var chapterDtos = _mapper.Map<List<ChapterDto>>(chapters);

            return new PagedResultDto<ChapterDto>(totalCount, chapterDtos);
        }

        public async Task<ResponseApi<ChapterDto>> CreateAsync(CreateUpdateChapterDto input)
        {
            var chapter = _mapper.Map<Chapter>(input);
            var result = await _chapterRepository.InsertAsync(chapter);
            var dto = _mapper.Map<ChapterDto>(result);
            return new ResponseApi<ChapterDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<ChapterDto>> UpdateAsync(Guid id, CreateUpdateChapterDto input)
        {
            var chapterDB = await _chapterRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (chapterDB == null) return new ResponseApi<ChapterDto> { Data = null, Success = false, Message = "Not found chapter" };
            var chapter = _mapper.Map(input, chapterDB);
            var result = await _chapterRepository.UpdateAsync(chapter);
            var dto = _mapper.Map<ChapterDto>(result);
            return new ResponseApi<ChapterDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var chapter = await(await _chapterRepository.GetQueryableAsync()).Include(x=>x.Lectures).FirstOrDefaultAsync(x => x.Id == id);
            if (chapter == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found chapter" };
            foreach (var lec in chapter.Lectures) await _lectureManger.DeleteAsync(lec.Id);
            await _chapterRepository.DeleteAsync(chapter);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetListChaptersAsync()
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());
            var queryable = await _chapterRepository.GetQueryableAsync();
            var chapters = new List<Chapter>();
            if (roles.Any(x => x.Name.ToUpper() == RoleConsts.Admin.ToUpper())) chapters = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime).Take(100));
            else chapters = await AsyncExecuter.ToListAsync(queryable.Where(c => c.CreatorId == _currentUser.GetId()).OrderByDescending(c => c.CreationTime).Take(100));
            var chapterDtos = _mapper.Map<List<LookupDto>>(chapters);
            return new PagedResultDto<LookupDto>(chapterDtos.Count, chapterDtos);
        }




        public async Task<PagedResultDto<CourseChaptersDto>> GetCourseChaptersAsync(Guid courseId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var userId = _currentUser.GetId();

            // ✅ كل إجابات المستخدم (لكل كويز)
            var userQuizAttempts = await (await _quizStudentRepository.GetQueryableAsync())
                .Where(qs => qs.UserId == userId)
                .Select(qs => new { qs.QuizId, qs.TryCount, qs.LectureId })
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

            var totalCount = await query.CountAsync();
            var chapters = await query
                .OrderBy(c => c.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var chapterInfoDtos = new List<CourseChaptersDto>();

            foreach (var c in chapters)
            {
                var lectureDtos = new List<LectureInfoDto>();

                foreach (var l in c.Lectures.Where(x => x.IsVisible))
                {
                    var media = await _mediaItemManager.GetAsync(l.Id);

                    var lectureTry = lectureTries.FirstOrDefault(x => x.LectureId == l.Id)
                        ?? new LectureTry
                        {
                            LectureId = l.Id,
                            UserId = userId,
                            MyTryCount = 0
                        };

                    // كل الكويزات بالترتيب
                    var quizzes = l.Quizzes.OrderBy(q => q.CreationTime).ToList();

                    QuizInfoDto quizDto;

                    if (quizzes.Any())
                    {
                        int currentQuizIndex = 0;

                        // 🔥 نحدد أول كويز لسه الطالب ما خلصش محاولاته فيه
                        for (int i = 0; i < quizzes.Count; i++)
                        {
                            var quiz = quizzes[i];
                            var userQuiz = userQuizAttempts.FirstOrDefault(q => q.QuizId == quiz.Id);

                            int usedTries = userQuiz?.TryCount ?? 0;
                            if (usedTries < quiz.QuizTryCount)
                            {
                                currentQuizIndex = i;
                                break;
                            }

                            // لو الطالب خلص كل المحاولات، ننتقل للكويز التالي
                            if (i == quizzes.Count - 1)
                                currentQuizIndex = quizzes.Count - 1; // آخر كويز
                        }

                        var nextQuiz = quizzes[currentQuizIndex];
                        var trys = await _lectureManger.UserTryCount(userId,l.Id, nextQuiz.Id);   
                        quizDto = new QuizInfoDto
                        {
                            QuizId = nextQuiz.Id,
                            Title = nextQuiz.Title,
                            QuestionsCount = nextQuiz.Questions.Count,
                            QuizTryCount = trys?.Data?.LectureTryCount??0,
                            TryedCount = trys?.Data?.MyTryCount??0,
                            IsSucces = trys?.Data?.IsSucces?? false,
                            AlreadyAnswer = userQuizAttempts.Any(q => q.LectureId == l.Id)
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

                    lectureDtos.Add(new LectureInfoDto
                    {
                        LectureId = l.Id,
                        Title = l.Title,
                        Content = l.Content,
                        VideoUrl = l.VideoUrl,
                        Quiz = quizDto
                    });

                    foreach (var dto in lectureDtos)
                    {
                        var lecPdfs = await _mediaItemManager.GetListAsync(l.Id);
                        foreach (var pdf in lecPdfs)
                            if (!pdf.IsImage) dto.PdfUrls.Add(pdf.Url);
                    }
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

            return new PagedResultDto<CourseChaptersDto>(totalCount, chapterInfoDtos);
        }





        public async Task<PagedResultDto<LookupDto>> GetChaptersByCourseLookUpAsync(Guid courseId)
        {
            // هات الـ Chapters اللي ليها نفس CourseId
            var queryable = await _chapterRepository.GetQueryableAsync();
            var chapters = await queryable
                .Where(c => c.CourseId == courseId)
                .ToListAsync();

            if (!chapters.Any())
                return new PagedResultDto<LookupDto>(0,new List<LookupDto>());

            // اعمل Map للـ DTOs
            var chapterDtos = _mapper.Map<List<LookupDto>>(chapters);

            return new PagedResultDto<LookupDto>(chapterDtos.Count(), chapterDtos);
        }

    }
}
