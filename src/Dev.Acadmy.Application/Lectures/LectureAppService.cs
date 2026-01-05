using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Questions;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Lectures
{
    public class LectureAppService:ApplicationService
    {
        private readonly LectureManager _lectureManager;
        private readonly QuizManager _quizManager;
        private readonly IMediaItemRepository _mediaItemRepository;
        public LectureAppService(IMediaItemRepository mediaItemRepository, QuizManager quizManager, LectureManager lectureManager)
        {
            _mediaItemRepository = mediaItemRepository;
            _quizManager = quizManager;
            _lectureManager = lectureManager;
        }
        [Authorize(AcadmyPermissions.Lectures.View)]
        public async Task<ResponseApi<LectureDto>> GetAsync(Guid id) => await _lectureManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Lectures.View)]
        public async Task<PagedResultDto<LectureDto>> GetListAsync(int pageNumber, int pageSize, string? search,Guid chapterId) => await _lectureManager.GetListAsync(pageNumber, pageSize, search,chapterId);
        [Authorize(AcadmyPermissions.Lectures.Create)]
        public async Task<ResponseApi<LectureDto>> CreateAsync(CreateUpdateLectureDto input) => await _lectureManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Lectures.Edit)]
        public async Task<ResponseApi<LectureDto>> UpdateAsync(Guid id, CreateUpdateLectureDto input) => await _lectureManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Lectures.Delete)]
        public async Task DeleteAsync(Guid id) => await _lectureManager.DeleteAsync(id);
        [Authorize]
        public async Task<ResponseApi<QuizDetailsDto>> GetQuizDetailsAsync(Guid refId, bool isExam)
        {
            // 1. استدعاء المانجر للحصول على بيانات الدومين الخام
            var quizDetailModel = await _quizManager.GetFullDetailsAsync(refId, isExam);

            // 2. جلب الصور (Cross-cutting concern يفضل بقاؤه في الـ Application Layer)
            var questionIds = quizDetailModel.Questions.Select(q => q.Id).ToList();
            var imagesDict = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(questionIds);

            // 3. Mapping إلى DTO
            var dto = new QuizDetailsDto
            {
                QuizId = quizDetailModel.Id,
                Title = quizDetailModel.Title,
                QuizTime = quizDetailModel.QuizTime,
                QuizTryCount = quizDetailModel.TryCount,
                Questions = quizDetailModel.Questions.Select(q => new QuestionDetailesDto
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
        [Authorize]
        public async Task<ResponseApi<QuizResultDto>> CorrectQuizAsync(QuizAnswerDto input,bool isExam) => await _quizManager.SubmitQuizAsync(input, isExam);
         [Authorize]
        public async Task<ResponseApi<LectureWithQuizzesDto>> GetLectureWithQuizzesAsync(Guid refId , bool isCourse) => await _lectureManager.GetLectureWithQuizzesAsync(refId , isCourse);
        
        // this in fuature i will take to admin panel
        [Authorize]
        public async Task<ResponseApi<LectureQuizResultDto>> GetLectureQuizResultsAsync(Guid lectureId) => await _quizManager.GetLectureQuizResultsAsync(lectureId);
        [AllowAnonymous]
        public async Task<ResponseApi<LectureTryDto>> GetUserTryCount(Guid userId, Guid lecId,Guid quizId) => await _lectureManager.UserTryCount(userId, lecId ,quizId);

    }
}
