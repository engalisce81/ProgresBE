using AutoMapper;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Microsoft.EntityFrameworkCore;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.Quizzes;

namespace Dev.Acadmy.Questions
{
    public class QuestionBankManager:DomainService
    {
        private readonly IRepository<QuestionBank> _questionbankRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Exam, Guid> _examRepositroy;
        private readonly ICurrentUser _currentUser;
        private readonly IIdentityUserRepository _userRepository;
        public QuestionBankManager(IIdentityUserRepository userRepository, ICurrentUser currentUser, IRepository<Exam, Guid> examRepositroy, IMapper mapper, IRepository<QuestionBank> questionbankRepository)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _examRepositroy = examRepositroy;
            _questionbankRepository = questionbankRepository;
            _mapper = mapper;
        }

        public async Task<QuestionBank?> GetByQuestionBank(Guid courseId) => await _questionbankRepository.FirstOrDefaultAsync(x => x.UserId == courseId);
        public async Task<ResponseApi<QuestionBankDto>> GetAsync(Guid id)
        {
            var questionbank = await _questionbankRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (questionbank == null) return new ResponseApi<QuestionBankDto> { Data = null, Success = false, Message = "Not found questionbank" };
            var dto = _mapper.Map<QuestionBankDto>(questionbank);
            return new ResponseApi<QuestionBankDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<QuestionBankDto>> GetListAsync(int pageNumber, int pageSize, string? search)
        {
            var roles = await _userRepository.GetRolesAsync(_currentUser.GetId());
            var queryable = await _questionbankRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Include(x => x.User).Where(c => c.Name.Contains(search) ||  c.User.Name.Contains(search));
            var questionbanks = new List<QuestionBank>();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            if (roles.Any(x => x.Name.ToUpper() == RoleConsts.Admin.ToUpper())) questionbanks = await AsyncExecuter.ToListAsync(queryable.Include(x => x.User   ).OrderByDescending(c => c.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            else questionbanks = await AsyncExecuter.ToListAsync(queryable.Where(c => c.CreatorId == _currentUser.GetId()).Include(x => x.User).OrderByDescending(c => c.Name).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var questionbankDtos = _mapper.Map<List<QuestionBankDto>>(questionbanks);
            return new PagedResultDto<QuestionBankDto>(totalCount, questionbankDtos);
        }

        public async Task<ResponseApi<QuestionBankDto>> CreateAsync(CreateUpdateQuestionBankDto input)
        {
            var questionbank = _mapper.Map<QuestionBank>(input);
            questionbank.UserId = _currentUser.GetId();
            var result = await _questionbankRepository.InsertAsync(questionbank,autoSave:true);
            var dto = _mapper.Map<QuestionBankDto>(result);
            return new ResponseApi<QuestionBankDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<QuestionBankDto>> UpdateAsync(Guid id, CreateUpdateQuestionBankDto input)
        {
            var questionbankDB = await _questionbankRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (questionbankDB == null) return new ResponseApi<QuestionBankDto> { Data = null, Success = false, Message = "Not found questionbank" };
            var questionbank = _mapper.Map(input, questionbankDB);
            questionbank.UserId = _currentUser.GetId();
            var result = await _questionbankRepository.UpdateAsync(questionbank);
            var dto = _mapper.Map<QuestionBankDto>(result);
            return new ResponseApi<QuestionBankDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var questionbank = await _questionbankRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (questionbank == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found questionbank" };
            await _questionbankRepository.DeleteAsync(questionbank);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetListMyBankAsync()
        {
            var banks =  await (await _questionbankRepository.GetQueryableAsync()).Where(x => x.CreatorId == _currentUser.GetId()).Select(x => new LookupDto { Id = x.Id, Name = x.Name }).ToListAsync();
            return new PagedResultDto<LookupDto>(banks.Count, banks);
        }


        public async Task<ResponseApi<List<QuestionBankWithQuestionsDto>>> GetQuestionsByBankIdsAsync(List<Guid> bankIds)
        {
            if (bankIds == null || !bankIds.Any())
            {
                return new ResponseApi<List<QuestionBankWithQuestionsDto>>
                {
                    Success = false,
                    Message = "No bank IDs provided.",
                    Data = null
                };
            }

            // نجيب البنوك مع الأسئلة والإجابات
            var banks = await (await _questionbankRepository.GetQueryableAsync())
                .Where(qb => bankIds.Contains(qb.Id))
                .Include(qb => qb.Questions)
                    .ThenInclude(q => q.QuestionAnswers)
                .Include(qb => qb.Questions)
                    .ThenInclude(q => q.QuestionType)
                .ToListAsync();

            if (!banks.Any())
            {
                return new ResponseApi<List<QuestionBankWithQuestionsDto>>
                {
                    Success = false,
                    Message = "No question banks found.",
                    Data = null
                };
            }

            var result = banks.Select(bank => new QuestionBankWithQuestionsDto
            {
                BankId = bank.Id,
                BankName = bank.Name,
                Questions = bank.Questions.Select(q => new Dev.Acadmy.Quizzes.QuestionDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    QuestionType = q.QuestionType.Name,
                    CorrectAnswers = q.QuestionAnswers
                        .Where(a => a.IsCorrect)
                        .Select(a => new AnswerDto
                        {
                            Id = a.Id,
                            Answer = a.Answer
                        }).ToList()
                }).ToList()
            }).ToList();

            return new ResponseApi<List<QuestionBankWithQuestionsDto>>
            {
                Success = true,
                Message = "Questions loaded successfully.",
                Data = result
            };
        }

    }
}
