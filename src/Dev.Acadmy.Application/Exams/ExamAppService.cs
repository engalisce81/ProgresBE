using AutoMapper;
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
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
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
        private readonly IMapper _mapper;
        private readonly IMediaItemRepository _mediaItemRepository;
        public ExamAppService(
            ExamManager examManager,
            ICurrentUser currentUser,
            IRepository<Exam, Guid> examRepository,
            IExamQuestionRepository examQuestionRepository,
            IExamStudentRepository examStudentRepository,
            IExamStudentAnswerRepository examStudentAnswerRepository,
            IdentityUserManager userManager,
            IRepository<IdentityUser, Guid> userRepository,
            IMapper mapper,
            IMediaItemRepository mediaItemRepository)
        {
            _examManager = examManager;
            _currentUser = currentUser;
            _examRepository = examRepository;
            _examQuestionRepository = examQuestionRepository;
            _examStudentRepository = examStudentRepository;
            _examStudentAnswerRepository = examStudentAnswerRepository;
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _mediaItemRepository = mediaItemRepository;
        }
        [Authorize(AcadmyPermissions.Exams.View)]
        public async Task<ResponseApi<ExamDto>> GetAsync(Guid id) => await _examManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Exams.View)]
        public async Task<PagedResultDto<ExamDto>> GetListAsync(int pageNumber, int pageSize, string? search, Guid courseId)
        {
            // 1. تجهيز الـ Queryable الأساسي مع الـ Include المطلوب
            var queryable = (await _examRepository.GetQueryableAsync())
                .Include(x => x.Course)
                .Where(x => x.CourseId == courseId); // استخدام البارامتر المرسل

            // 2. تطبيق الفلترة بالبحث (Search)
            if (!string.IsNullOrWhiteSpace(search))
            {
                queryable = queryable.Where(c => c.Name.Contains(search) || c.Course.Name.Contains(search));
            }

            // 3. تطبيق الفلترة بناءً على الصلاحيات (Authorization Filter)
            // ملاحظة: الأفضل استخدام CurrentUser.IsInRole بدلاً من جلب الأدوار يدوياً للأداء
            if (!CurrentUser.IsInRole(RoleConsts.Admin.ToLower()))
            {
                queryable = queryable.Where(c => c.CreatorId == CurrentUser.GetId());
            }

            // 4. حساب العدد الإجمالي قبل الـ Pagination
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // 5. جلب البيانات مع الـ Pagination والترتيب
            var exams = await AsyncExecuter.ToListAsync(
                queryable.OrderByDescending(c => c.CreationTime) // الترتيب بالتاريخ عادة أفضل للامتحانات
                         .Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
            );

            // 6. المابينج والإرجاع
            var examDtos = _mapper.Map<List<ExamDto>>(exams);

            return new PagedResultDto<ExamDto>(totalCount, examDtos);
        }
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
        [Authorize]
        public async Task<ExamStudentStatusDto> GetExamStudentStatusAsync(Guid examId)
        {
            var userId = CurrentUser.GetId(); // الحصول على الـ UserId من الـ Session
            var examStudent = await _examStudentRepository.FirstOrDefaultAsync(x =>
                x.ExamId == examId && x.UserId == userId);

            if (examStudent == null) throw new EntityNotFoundException();

            return new ExamStudentStatusDto
            {
                ExamId = examStudent.ExamId,
                IsPassed = examStudent.IsPassed,
                Score = examStudent.Score,
                IsCertificateIssued = examStudent.IsCertificateIssued,
                CanRequestNow = examStudent.CanRequestCertificate(),
                NextAvailableDate = examStudent.LastCertificateRequestDate?.AddDays(1)
            };
        }

        [Authorize]
        public async Task<PagedResultDto<ExamStudentDto>> GetExamParticipantsAsync(int pageNumber, int pageSize, string? search, Guid examId)
        {
            // 1. الوصول إلى Queryable من المستودع مع تضمين البيانات المرتبطة
            var studentQuery = await _examStudentRepository.GetQueryableAsync();
    
            // استخدام Include لجلب بيانات المستخدم والامتحان في استعلام واحد (Eager Loading)
            var query = studentQuery
                .Include(x => x.User)
                .Include(x => x.Exam)
                .Where(x => x.ExamId == examId);

            // 2. فلتر البحث (Search Filter)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(x =>
                    x.User.Name.ToLower().Contains(lowerSearch) ||
                    x.User.Surname.ToLower().Contains(lowerSearch) ||
                    x.User.UserName.ToLower().Contains(lowerSearch));
            }

            // 3. حساب العدد الإجمالي (Total Count)
            var totalCount = await AsyncExecuter.CountAsync(query);

            // 4. التقسيم لصفحات والترتيب (يفضل الترتيب دائماً عند استخدام Paging)
            var skipCount = (pageNumber - 1) * pageSize;
            var list = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.FinishedAt) // ترتيب تنازلي حسب وقت الانتهاء
                     .PageBy(skipCount, pageSize)
            );

            // 5. جلب روابط الصور باستخدام Dictionary
            var userIds = list.Select(x => x.UserId).ToList();
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(userIds);

            // 6. التحويل إلى DTO (Mapping)
            var dtos = list.Select(x => new ExamStudentDto
            {
                ExamId = x.ExamId,
                UserId = x.UserId,
                // دمج الاسم واللقب بشكل صحيح
                FullName = $"{x.User.Name} {x.User.Surname}".Trim(), 
                // سحب الرابط من القاموس بناءً على UserId، وإذا لم يوجد نضع صورة افتراضية
                LogoUrl = mediaItemDic.TryGetValue(x.UserId, out var url) ? url : "/assets/images/default-avatar.png",
                Score = x.Score,
                TryCount = x.TryCount,
                IsPassed = x.IsPassed,
                FinishedAt = x.FinishedAt,
                IsCertificateIssued = x.IsCertificateIssued
            }).ToList();

            return new PagedResultDto<ExamStudentDto>(totalCount, dtos);
        }
    }
}
