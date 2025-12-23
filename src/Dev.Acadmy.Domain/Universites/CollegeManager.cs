using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Dev.Acadmy.Response;
using Dev.Acadmy.LookUp;
using Volo.Abp.Users;
namespace Dev.Acadmy.Universites
{
    public class CollegeManager :DomainService
    {
        private readonly IRepository<College ,Guid> _collegeRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly GradeLevelManager _gradeLevelManager;
        private readonly SubjectManager _subjectManager;
        private readonly IRepository<University, Guid> _universityRepository;
        private readonly IRepository<Term, Guid> _termRepository;
        public CollegeManager(SubjectManager subjectManager, IRepository<Term, Guid> termRepository, IRepository<University, Guid> universityRepository, GradeLevelManager gradeLevelManager, ICurrentUser currentUser, IMapper mapper, IRepository<College,Guid> collegeRepository) 
        {
            _subjectManager = subjectManager;
            _termRepository = termRepository;
            _universityRepository = universityRepository;
            _gradeLevelManager = gradeLevelManager;
            _currentUser = currentUser;
            _collegeRepository = collegeRepository;
            _mapper = mapper;
        }

        public async Task<ResponseApi<CollegeDto>> GetAsync(Guid id)
        {
            var college = await (await _collegeRepository.GetQueryableAsync()).Include(x=>x.University).FirstOrDefaultAsync(x => x.Id == id);
            if (college == null)  return  new ResponseApi<CollegeDto> { Data = null, Success = false, Message = "Not found college" };
            var dto = _mapper.Map<CollegeDto>(college);
            return new ResponseApi<CollegeDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<CollegeDto>> GetListAsync(int pageNumber, int pageSize, string? search,Guid universityId)
        {
            var queryable = (await _collegeRepository.GetQueryableAsync());
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Include(x => x.University).Where(c => c.Name.Contains(search));  
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var colleges = await AsyncExecuter.ToListAsync(queryable.Include(x => x.University).OrderByDescending(c => c.CreationTime).Where(x=>x.UniversityId == universityId).Skip((pageNumber - 1) * pageSize) .Take(pageSize) );
            var collegeDtos = _mapper.Map<List<CollegeDto>>(colleges);
            return new PagedResultDto<CollegeDto>(totalCount, collegeDtos);
        }

        public async Task<ResponseApi<CollegeDto>> CreateAsync(CreateUpdateCollegeDto input)
        {
            var college= _mapper.Map<College>(input);
            var result = await _collegeRepository.InsertAsync(college);
            await CreateGraeLevels(input.GradeLevelCount,result.Id);
            var dto = _mapper.Map<CollegeDto>(result);
            return new ResponseApi<CollegeDto> { Data=dto,Success=true ,Message= "save succeess"};
        }

        public async Task<ResponseApi<CollegeDto>> UpdateAsync(Guid id, CreateUpdateCollegeDto input)
        {
            var collegeDB = await (await _collegeRepository.GetQueryableAsync()).Include(x=>x.GradeLevels).FirstOrDefaultAsync(x => x.Id == id);
            if (collegeDB == null) return new ResponseApi<CollegeDto> { Data = null, Success = false, Message = "Not found college" };
            var college = _mapper.Map(input, collegeDB);
            var result = await _collegeRepository.UpdateAsync(college);
            var dto = _mapper.Map<CollegeDto>(result);
            return new ResponseApi<CollegeDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var college = await (await _collegeRepository.GetQueryableAsync()).Include(x => x.GradeLevels).FirstOrDefaultAsync(x => x.Id == id);
            if (college == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found college" };
            var subjects = await _subjectManager.GetSubjectsWithCollegeListAsync(id);
            foreach(var subject in subjects.Items) await _subjectManager.DeleteAsync(subject.Id);
            await DeleteGraeLevels(college.GradeLevels.ToList());
            await _collegeRepository.DeleteAsync(college);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetCollegesListAsync(Guid universityId)
        {
            var queryable = await _universityRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var colleges = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime).Where(x=>x.Id == universityId).SelectMany(x=>x.Colleges));
            var collegeDtos = _mapper.Map<List<LookupDto>>(colleges);
            return new PagedResultDto<LookupDto>(totalCount, collegeDtos);
        }

        public async Task<PagedResultDto<LookupDto>> GetGradeLevelListAsync(Guid collegeId)
        {
            var queryable = await _collegeRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var gradeLevels = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime).Where(x => x.Id == collegeId).SelectMany(x => x.GradeLevels));
            var gradeLevelDtos = _mapper.Map<List<LookupDto>>(gradeLevels);
            return new PagedResultDto<LookupDto>(totalCount, gradeLevelDtos);
        }

        public async Task<PagedResultDto<LookupDto>> GetTermListAsync()
        {
            var queryable = await _termRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var terms = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime));
            var termDtos = _mapper.Map<List<LookupDto>>(terms);
            return new PagedResultDto<LookupDto>(totalCount, termDtos);
        }

        private async Task CreateGraeLevels(int countGradeLevel,Guid collegeId)
        { 
            for (int x = 0; x < countGradeLevel; x++) await _gradeLevelManager.CreateAsync(new CreateUpdateGradeLevelDto { Name = $"Grade Level {x + 1}" ,CollegeId=collegeId});
        }
        private async Task DeleteGraeLevels(List<GradeLevel> gradeLevels)
        {
            foreach (var gradeLevel in gradeLevels) await _gradeLevelManager.DeleteAsync(gradeLevel.Id);
        }
    }
}
