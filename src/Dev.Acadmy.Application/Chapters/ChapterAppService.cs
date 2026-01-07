using AutoMapper;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Chapters
{
    public class ChapterAppService : ApplicationService, IChapterAppService
    {
        private readonly ChapterManager _chapterManager;
        private readonly LectureManager _lectureManager;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly IRepository<QuizStudent, Guid> _quizStudentRepository;
        public ChapterAppService(
        ChapterManager chapterManager,
            LectureManager lectureManager,
            IMediaItemRepository mediaItemRepository,
            IChapterRepository chapterRepository,
            ICurrentUser currentUser,
            IRepository<IdentityUser, Guid> userRepository,
            IRepository<QuizStudent, Guid> quizStudentRepository)
        {
            _chapterManager = chapterManager;
            _lectureManager = lectureManager;
            _mediaItemRepository = mediaItemRepository;
            _chapterRepository = chapterRepository;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _quizStudentRepository = quizStudentRepository;
        }
        [Authorize(AcadmyPermissions.Chapters.View)]
        public async Task<ResponseApi<ChapterDto>> GetAsync(Guid id) => await _chapterManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Chapters.View)]
        public async Task<PagedResultDto<ChapterDto>> GetListAsync(int pageNumber, int pageSize, string? search,Guid courseId) => await _chapterManager.GetListAsync(pageNumber, pageSize, search,courseId);
        [Authorize(AcadmyPermissions.Chapters.Create)]
        public async Task<ResponseApi<ChapterDto>> CreateAsync(CreateUpdateChapterDto input) => await _chapterManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Chapters.Edit)]
        public async Task<ResponseApi<ChapterDto>> UpdateAsync(Guid id, CreateUpdateChapterDto input) => await _chapterManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Chapters.Delete)]
        public async Task DeleteAsync(Guid id) => await _chapterManager.DeleteAsync(id);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetListChaptersAsync() => await _chapterManager.GetListChaptersAsync();
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetChaptersByCourseLookUpAsync(Guid courseId) => await _chapterManager.GetChaptersByCourseLookUpAsync(courseId);
        [Authorize]
        public async Task<PagedResultDto<CourseChaptersDto>> GetCourseChaptersAsync(Guid courseId, int pageNumber, int pageSize)
        {
            // A. التحقق الأولي
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            int skipCount = (pageNumber - 1) * pageSize;
            var userId = _currentUser.GetId();

            var (chapters, totalCount) = await _chapterRepository.GetPagedChaptersWithDetailsAsync(courseId, skipCount, pageSize);
            // C. التحضير لجلب البيانات المجمعة (Preparing IDs)
            var allLectures = chapters.SelectMany(c => c.Lectures).Where(x => x.IsVisible).ToList();
            var allLectureIds = allLectures.Select(l => l.Id).Distinct().ToList();
            var creatorIds = chapters.Select(c => c.Course.UserId).Distinct().ToList();

            // تحديد الكويز "النشط" لكل محاضرة (In-Memory Logic)
            var userQuizAttempts = await (await _quizStudentRepository.GetQueryableAsync())
                .Where(qs => qs.UserId == userId &&
                             qs.LectureId != null && // التأكد من أن المحاضرة ليست نال
                             allLectureIds.Contains(qs.LectureId.Value)) // استخدام .Value بأمان بعد التحقق
                .Select(qs => new
                {
                    qs.QuizId,
                    qs.TryCount,
                    LectureId = qs.LectureId.Value // نقوم بعمل Cast هنا للقيمة
                })
                .ToListAsync();

            var lectureToActiveQuizMap = new Dictionary<Guid, Guid>();
            foreach (var lec in allLectures)
            {
                var activeQuizId = lec.Quizzes.OrderBy(q => q.CreationTime)
                    .FirstOrDefault(q => (userQuizAttempts.FirstOrDefault(a => a.QuizId == q.Id)?.TryCount ?? 0) < q.QuizTryCount)?.Id
                    ?? lec.Quizzes.OrderBy(q => q.CreationTime).LastOrDefault()?.Id;

                if (activeQuizId.HasValue) lectureToActiveQuizMap[lec.Id] = activeQuizId.Value;
            }

            // D. جلب البيانات من الـ Repositories والـ Manager دفعة واحدة (Parallel Tasking)
            var lectureMediaDict = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(allLectureIds);
            var userLogosDict = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(creatorIds);
            var users = await _userRepository.GetListAsync(u => creatorIds.Contains(u.Id));
            var lecturesStatusDict = await _lectureManager.GetLecturesStatusAsync(userId, allLectureIds, lectureToActiveQuizMap.Values.ToList());

            // E. بناء النتيجة النهائية (Mapping)
            var chapterInfoDtos = chapters.Select(c => new CourseChaptersDto
            {
                CourseId = c.CourseId,
                CourseName = c.Course.Name,
                ChapterId = c.Id,
                ChapterName = c.Name,
                UserId = c.Course.UserId,
                UserName = users.FirstOrDefault(u => u.Id == c.Course.UserId)?.Name ?? string.Empty,
                LogoUrl = userLogosDict.GetValueOrDefault(c.Course.UserId) ?? string.Empty,
                LectureCount = c.Lectures.Count(x => x.IsVisible),
                Lectures = c.Lectures.Where(x => x.IsVisible).Select(l =>
                {
                    lecturesStatusDict.TryGetValue(l.Id, out var status);
                    lectureToActiveQuizMap.TryGetValue(l.Id, out var activeQuizId);
                    var activeQuiz = l.Quizzes.FirstOrDefault(q => q.Id == activeQuizId);

                    return new LectureInfoDto
                    {
                        LectureId = l.Id,
                        Title = l.Title,
                        Content = l.Content,
                        DriveVideoUrl = l.DriveVideoUrl,
                        YouTubeVideoUrl = l.YouTubeVideoUrl,
                        HasDriveVideo = l.HasDriveVideo,
                        HasYouTubeVideo = l.HasYouTubeVideo,
                        IsQuizRequired = l.IsRequiredQuiz,
                        PdfUrls = lectureMediaDict.ContainsKey(l.Id) ? new List<string> { lectureMediaDict[l.Id] } : new List<string>(),
                        Quiz = activeQuiz == null ? new QuizInfoDto { Title = "لا يوجد كويز متاح", QuizId = Guid.Empty } : new QuizInfoDto
                        {
                            QuizId = activeQuiz.Id,
                            Title = activeQuiz.Title,
                            QuestionsCount = activeQuiz.Questions.Count,
                            QuizTryCount = status?.LectureTryCount ?? 0,
                            TryedCount = status?.MyTryCount ?? 0,
                            IsSucces = status?.IsSucces ?? false,
                            AlreadyAnswer = userQuizAttempts.Any(qa => qa.LectureId == l.Id)
                        }
                    };
                }).ToList()
            }).ToList();

            return new PagedResultDto<CourseChaptersDto>(totalCount, chapterInfoDtos);
        }

    }
}
