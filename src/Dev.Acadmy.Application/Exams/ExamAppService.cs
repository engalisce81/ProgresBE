using Dev.Acadmy.Dtos.Response.Exams;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Interfaces.Dev.Acadmy.Exams;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Exams
{
    public class ExamAppService:ApplicationService
    {
        private readonly ExamManager _examManager;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<Exam, Guid> _examRepository; 
        private readonly IExamQuestionRepository _examQuestionRepository;
        private readonly IExamStudentRepository _examStudentRepository;
        private readonly IExamStudentAnswerRepository _examStudentAnswerRepository;
        private readonly IdentityUserManager _userManager; // للتحقق من دور الأدمن
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        public ExamAppService(
            ExamManager examManager,
            ICurrentUser currentUser,
            IRepository<Exam, Guid> examRepository,
            IExamQuestionRepository examQuestionRepository,
            IExamStudentRepository examStudentRepository,
            IExamStudentAnswerRepository examStudentAnswerRepository,
            IdentityUserManager userManager,
            IRepository<IdentityUser, Guid> userRepository          )
        {
            _examManager = examManager;
            _currentUser = currentUser;
            _examRepository = examRepository;
            _examQuestionRepository = examQuestionRepository;
            _examStudentRepository = examStudentRepository;
            _examStudentAnswerRepository = examStudentAnswerRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }
        [Authorize(AcadmyPermissions.Exams.View)]
        public async Task<ResponseApi<ExamDto>> GetAsync(Guid id) => await _examManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Exams.View)]
        public async Task<PagedResultDto<ExamDto>> GetListAsync(int pageNumber, int pageSize, string? search) => await _examManager.GetListAsync(pageNumber, pageSize, search);
        [Authorize(AcadmyPermissions.Exams.Create)]
        public async Task<ResponseApi<ExamDto>> CreateAsync(CreateUpdateExamDto input) => await _examManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Exams.Edit)]
        public async Task<ResponseApi<ExamDto>> UpdateAsync(Guid id, CreateUpdateExamDto input) => await _examManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Exams.Delete)]
        public async Task DeleteAsync(Guid id) => await _examManager.DeleteAsync(id);
        [Authorize]
        public async Task AddQuestionToExamAsync(CreateUpdateExamQuestionDto input) => await _examManager.AddQuestionToExam(input);
        [Authorize]
        public async Task<PagedResultDto<ExamQuestionsDto>> GetQuestionsFromBankAsync(List<Guid> bankIds, Guid examId)=> await _examManager.GetQuestionsFromBankAsync(bankIds, examId);
        [Authorize]
        public async Task<ResponseApi<ExamStudentResultDto>> GetStudentExamResultAsync(Guid examId, Guid? userId = null)
        {
            var currentUserId = _currentUser.GetId();

            // التحقق من الصلاحيات: لو طلب userId مختلف وهو مش أدمن نرفض الطلب
            Guid targetUserId = userId ?? currentUserId;
            if (targetUserId != currentUserId && !await _userManager.IsInRoleAsync(await _userRepository.GetAsync(currentUserId), "admin"))
            {
                throw new UserFriendlyException("غير مسموح لك بالاطلاع على نتائج طلاب آخرين");
            }

            // جلب سجل المحاولة مع بيانات الامتحان
            var examStudent = await (await _examStudentRepository.GetQueryableAsync())
                .Include(x => x.Exam)
                .FirstOrDefaultAsync(x => x.ExamId == examId && x.UserId == targetUserId);

            if (examStudent == null)
                throw new UserFriendlyException("لم يتم العثور على سجل لهذه المحاولة");

            var answers = await (await _examStudentAnswerRepository.GetQueryableAsync())
                .Include(x => x.Question)
                    .ThenInclude(q => q.QuestionAnswers) // لجلب الإجابة الصحيحة والخيارات
                .Include(x => x.Question)
                    .ThenInclude(q => q.QuestionType)
                .Where(x => x.ExamStudentId == examStudent.Id)
                .ToListAsync();

            var result = new ExamStudentResultDto
            {
                ExamId = examStudent.ExamId,
                ExamTitle = examStudent.Exam?.Name??string.Empty,
                StudentScore = examStudent.Score,
                IsPassed = examStudent.IsPassed,
                FinishedAt = examStudent.FinishedAt,
                Answers = answers.Select(a => {
                    // استخراج الإجابة الصحيحة من قاعدة البيانات
                    var correctAnswer = a.Question.QuestionAnswers.FirstOrDefault(qa => qa.IsCorrect);

                    return new ExamAnswerDetailDto
                    {
                        QuestionId = a.QuestionId,
                        QuestionText = a.Question?.Title?? string.Empty,
                        QuestionType = a.Question?.QuestionType?.Name?? string.Empty,

                        // إجابة الطالب
                        StudentTextAnswer = a.TextAnswer,
                        StudentSelectedAnswerId = a.SelectedAnswerId,

                        // الإجابة الصحيحة (للمقارنة في الواجهة)
                        CorrectSelectedAnswerId = correctAnswer?.Id,
                        CorrectTextAnswer = correctAnswer?.Answer,

                        // قائمة الخيارات كاملة
                        AllOptions = a.Question.QuestionAnswers.Select(o => new Dtos.Response.Exams.ExamQuestionAnswerDto
                        {
                            Id = o.Id,
                            AnswerText = o.Answer,
                            IsCorrect = o.IsCorrect
                        }).ToList(),

                        ScoreObtained = a.ScoreObtained,
                        IsCorrect = a.IsCorrect
                    };
                }).ToList()
            };

            return new ResponseApi<ExamStudentResultDto> { Data = result, Success = true };
        }
    }
}
