using AutoMapper;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Questions;
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

namespace Dev.Acadmy.Exams
{
    public class ExamManager:DomainService
    {
        private readonly IRepository<Exam ,Guid> _examRepository;
        private readonly IRepository<Question, Guid> _questionRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<QuestionBank, Guid> _questionBankRepository;
        private readonly IIdentityUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<ExamQuestionBank, Guid> _examQuestionBankRepository;
        private readonly IRepository<ExamQuestion, Guid> _examQuestionRepository;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IMediaItemRepository _mediaItemRepository;
        public ExamManager(IMediaItemRepository mediaItemRepository, MediaItemManager mediaItemManager, IRepository<ExamQuestion, Guid> examQuestionRepository, IRepository<ExamQuestionBank, Guid> examQuestionBankRepository, ICurrentUser currentUser , IIdentityUserRepository userRepository, IRepository<QuestionBank, Guid> questionBankRepository, IMapper mapper, IRepository<Exam,Guid> examRepository , IRepository<Question, Guid> questionRepository)
        {
            _mediaItemRepository = mediaItemRepository;
            _mediaItemManager = mediaItemManager;
            _examQuestionRepository = examQuestionRepository;
            _examQuestionBankRepository = examQuestionBankRepository;
            _currentUser = currentUser;
            _userRepository = userRepository;
            _questionBankRepository = questionBankRepository;
            _mapper = mapper;
            _questionRepository = questionRepository;
            _examRepository = examRepository;
        }

      

        public async Task<ResponseApi<ExamDto>> GetAsync(Guid id)
        {
            var exam = await _examRepository.GetAsync(id);
            if (exam == null) return new ResponseApi<ExamDto> { Data = null, Success = false, Message = "Not found exam" };
            var dto = _mapper.Map<ExamDto>(exam);
            return new ResponseApi<ExamDto> { Data = dto, Success = true, Message = "load succeess" };
        }


        public async Task<PagedResultDto<ExamDto>> GetListAsync(int pageNumber, int pageSize, string? search,Guid courseId)
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());
            var queryable = await _examRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Include(x => x.Course).Where(c => c.Name.Contains(search) || c.Course.Name.Contains(search));
            var exams = new List<Exam>();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            if (roles.Any(x => x.Name.ToUpper() == RoleConsts.Admin.ToUpper())) exams = await AsyncExecuter.ToListAsync(queryable.Include(x => x.Course).OrderByDescending(c => c.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            else exams = await AsyncExecuter.ToListAsync(queryable.Where(c => c.CreatorId == _currentUser.GetId()).Include(x => x.Course).OrderByDescending(c => c.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var examDtos = _mapper.Map<List<ExamDto>>(exams);
            return new PagedResultDto<ExamDto>(totalCount, examDtos);
        }

        public async Task<ResponseApi<ExamDto>> CreateAsync(CreateUpdateExamDto input)
        {
            var exam = _mapper.Map<Exam>(input);
            var result = await _examRepository.InsertAsync(exam, autoSave: true);
           // await CreateRelation(result, input);
            var dto = _mapper.Map<ExamDto>(result);
            return new ResponseApi<ExamDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<ExamDto>> UpdateAsync(Guid id, CreateUpdateExamDto input)
        {
            var examDB = await _examRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (examDB == null) return new ResponseApi<ExamDto> { Data = null, Success = false, Message = "Not found exam" };
            var exam = _mapper.Map(input, examDB);
            var result = await _examRepository.UpdateAsync(exam);
           // await DeleteRelation(id);
           // await CreateRelation(result, input);
            var dto = _mapper.Map<ExamDto>(result);
            return new ResponseApi<ExamDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var exam = await _examRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (exam == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found exam" };
         //   await DeleteRelation(id);
            await _examRepository.DeleteAsync(exam);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }


        public async Task<PagedResultDto<ExamQuestionsDto>> GetQuestionsFromBankAsync(List<Guid> bankIds, Guid? examId)
        {
            var queryableQuestions = await _questionRepository.GetQueryableAsync();
            List<Question> finalQuestionsList;

            // 1. تحديد قائمة الأسئلة الأساسية
            if (bankIds != null && bankIds.Any())
            {
                // لو مبعوث بنوك.. هات الأسئلة اللي جواها
                finalQuestionsList = await queryableQuestions
                    .Include(q => q.QuestionType)
                    .Include(q => q.QuestionAnswers)
                    .Where(q => bankIds.Contains((Guid)q.QuestionBankId))
                    .ToListAsync();
            }
            else if (examId.HasValue)
            {
                // لو البنوك فاضية بس فيه امتحان.. هات أسئلة الامتحان بس
                var examQuestionIds = await (await _examQuestionRepository.GetQueryableAsync())
                    .Where(x => x.ExamId == examId.Value)
                    .Select(x => x.QuestionId)
                    .ToListAsync();

                finalQuestionsList = await queryableQuestions
                    .Include(q => q.QuestionType)
                    .Include(q => q.QuestionAnswers)
                    .Where(q => examQuestionIds.Contains(q.Id))
                    .ToListAsync();
            }
            else
            {
                // لو الاتنين فاضيين رجع قائمة فاضية أو تصرف حسب منطق البزنس عندك
                return new PagedResultDto<ExamQuestionsDto>(0, new List<ExamQuestionsDto>());
            }

            // 2. جلب الأسئلة المختارة في الامتحان (لو الـ ExamId موجود)
            var selectedQuestionIds = new List<Guid>();
            if (examId.HasValue)
            {
                selectedQuestionIds = await (await _examQuestionRepository.GetQueryableAsync())
                    .Where(x => x.ExamId == examId.Value)
                    .Select(x => x.QuestionId)
                    .ToListAsync();
            }

            // 3. جلب الميديا دفعة واحدة (Batch)
            var allQuestionIds = finalQuestionsList.Select(x => x.Id).ToList();
            var mediaDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(allQuestionIds);

            // 4. تحويل لـ DTO
            var dtos = finalQuestionsList.Select(question => new ExamQuestionsDto
            {
                Id = question.Id,
                Tittle = question.Title,
                QuestionType = question.QuestionType?.Name ?? string.Empty,
                logoUrl = mediaDic.GetValueOrDefault(question.Id) ?? string.Empty,
                IsSelected = selectedQuestionIds.Contains(question.Id), // هتكون true لو السؤال في الامتحان
                QuestionAnswers = question.QuestionAnswers.Select(qa => new ExamQuestionAnswerDto
                {
                    AnswerId = qa.Id,
                    Answer = qa.Answer,
                    IsSelected = qa.IsCorrect
                }).ToList()
            }).ToList();

            return new PagedResultDto<ExamQuestionsDto>(dtos.Count, dtos);
        }

        public async Task AddQuestionToExam(CreateUpdateExamQuestionDto input)
        {
            // --- أولاً: التعامل مع الأسئلة (ExamQuestions) ---

            // 1. جلب جميع الارتباطات الحالية لهذا الامتحان من الجدول الوسيط
            var currentQuestions = await (await _examQuestionRepository.GetQueryableAsync())
                .Where(x => x.ExamId == input.ExamId)
                .ToListAsync();

            // 2. تحديد الارتباطات التي يجب حذفها (الموجودة في القاعدة وليست في القائمة المرسلة)
            var questionsToDelete = currentQuestions
                .Where(x => !input.QuestionIds.Contains(x.QuestionId))
                .ToList();

            if (questionsToDelete.Any())
            {
                await _examQuestionRepository.DeleteManyAsync(questionsToDelete);
            }

            // 3. تحديد الأسئلة الجديدة التي يجب إضافتها (المرسلة وليست موجودة مسبقاً)
            var existingQuestionIds = currentQuestions.Select(x => x.QuestionId).ToList();
            var newQuestionIds = input.QuestionIds.Except(existingQuestionIds).ToList();

            if (newQuestionIds.Any())
            {
                var newEntries = newQuestionIds.Select(qId => new ExamQuestion
                {
                    ExamId = input.ExamId,
                    QuestionId = qId
                }).ToList();

                await _examQuestionRepository.InsertManyAsync(newEntries, autoSave: true);
            }


            // --- ثانياً: التعامل مع بنوك الأسئلة (ExamQuestionBanks) ---

            // 1. جلب الارتباطات الحالية لبنوك الأسئلة
            var currentBanks = await (await _examQuestionBankRepository.GetQueryableAsync())
                .Where(x => x.ExamId == input.ExamId)
                .ToListAsync();

            // 2. تحديد البنوك التي يجب حذف ارتباطها
            var banksToDelete = currentBanks
                .Where(x => !input.QuestionBankIds.Contains(x.QuestionBankId))
                .ToList();

            if (banksToDelete.Any())
            {
                await _examQuestionBankRepository.DeleteManyAsync(banksToDelete);
            }

            // 3. تحديد البنوك الجديدة للإضافة
            var existingBankIds = currentBanks.Select(x => x.QuestionBankId).ToList();
            var newBankIds = input.QuestionBankIds.Except(existingBankIds).ToList();

            if (newBankIds.Any())
            {
                var newBankEntries = newBankIds.Select(bId => new ExamQuestionBank
                {
                    ExamId = input.ExamId,
                    QuestionBankId = bId
                }).ToList();

                await _examQuestionBankRepository.InsertManyAsync(newBankEntries, autoSave: true);
            }
        }

    }
}
