using AutoMapper;
using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Universites
{
    public class SubjectManager : DomainService
    {
        private readonly IRepository<Subject ,Guid> _subjectRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<College, Guid> _collegeRepository;
        private readonly IIdentityUserRepository _userRepository;
        private readonly CourseManager _courseManager;
        private readonly IRepository<GradeLevel, Guid> _gradeLevelRepository;
        private readonly IRepository<Entities.Courses.Entities.Course> _courseRepository;
        private readonly IRepository<MediaItem, Guid> _mediaItemRepository;
        public SubjectManager(IRepository<MediaItem, Guid> mediaItemRepository, IRepository<Entities.Courses.Entities.Course> courseRepository, IRepository<GradeLevel, Guid> gradeLevelRepository, CourseManager courseManager, IIdentityUserRepository userRepository, IRepository<College, Guid> collegeRepository, ICurrentUser currentUser, IMapper mapper, IRepository<Subject , Guid> subjectRepository)
        {
            _mediaItemRepository = mediaItemRepository;
            _courseRepository = courseRepository;
            _gradeLevelRepository = gradeLevelRepository;
            _courseManager = courseManager;
            _userRepository = userRepository;
            _collegeRepository = collegeRepository;
            _currentUser = currentUser;
            _subjectRepository = subjectRepository;
            _mapper = mapper;
        }

        public async Task<ResponseApi<SubjectDto>> GetAsync(Guid id)
        {
            var subject = await (await _subjectRepository.GetQueryableAsync()).Include(x=>x.GradeLevel).ThenInclude(x=>x.College).ThenInclude(x=>x.University).Include(x=>x.Term).FirstOrDefaultAsync(x => x.Id == id);
            if (subject == null) return new ResponseApi<SubjectDto> { Data = null, Success = false, Message = "Not found subject" };
            var dto = _mapper.Map<SubjectDto>(subject);
            dto.CollegeId = subject?.GradeLevel?.CollegeId?? new Guid();
            dto.CollegeName =subject?.GradeLevel?.College.Name?? string.Empty;
            dto.UniversityId = subject?.GradeLevel?.College?.UniversityId ?? new Guid();
            dto.UniversityName = subject?.GradeLevel?.College?.University?.Name?? string.Empty;
            return new ResponseApi<SubjectDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<SubjectDto>> GetListAsync(int pageNumber, int pageSize, string? search)
        {
            var queryable = await _subjectRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Include(x => x.GradeLevel).ThenInclude(x => x.College).ThenInclude(x => x.University).Include(x => x.Term).Where(c => c.Name.Contains(search));
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var subjects = await AsyncExecuter.ToListAsync(queryable.Include(x => x.GradeLevel).ThenInclude(x => x.College).ThenInclude(x => x.University).OrderByDescending(c => c.CreationTime).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var subjectDtos = new List<SubjectDto>();
            foreach(var subject in subjects)
            {
                var dto = _mapper.Map<SubjectDto>(subject);
                dto.CollegeId = subject?.GradeLevel?.CollegeId ?? new Guid();
                dto.CollegeName = subject?.GradeLevel?.College.Name ?? string.Empty;
                dto.UniversityId = subject?.GradeLevel?.College?.UniversityId ?? new Guid();
                dto.UniversityName = subject?.GradeLevel?.College?.University?.Name ?? string.Empty;
                subjectDtos.Add(dto);
            } 
            return new PagedResultDto<SubjectDto>(totalCount, subjectDtos);
        }

        public async Task<ResponseApi<SubjectDto>> CreateAsync(CreateUpdateSubjectDto input)
        {
            var subject = _mapper.Map<Subject>(input);
            var result = await _subjectRepository.InsertAsync(subject);
            var dto = _mapper.Map<SubjectDto>(result);
            return new ResponseApi<SubjectDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<SubjectDto>> UpdateAsync(Guid id, CreateUpdateSubjectDto input)
        {
            var subjectDB = await _subjectRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (subjectDB == null) return new ResponseApi<SubjectDto> { Data = null, Success = false, Message = "Not found subject" };
            var subject = _mapper.Map(input, subjectDB);
            var result = await _subjectRepository.UpdateAsync(subject);
            var dto = _mapper.Map<SubjectDto>(result);
            return new ResponseApi<SubjectDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var subject = await(await _subjectRepository.GetQueryableAsync()).Include(x=>x.Courses).FirstOrDefaultAsync(x => x.Id == id);
            if (subject == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found subject" };
            foreach (var course in subject.Courses) await _courseManager.DeleteAsync(course.Id);
            await _subjectRepository.DeleteAsync(subject);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetSubjectsListAsync()
        {
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());
            var termId = currentUser.GetProperty<Guid>(SetPropConsts.TermId);
            var queryable = await _subjectRepository.GetQueryableAsync();
            queryable = queryable.Include(x=>x.GradeLevel).Where(s => s.TermId == termId);
            var subjects = await queryable.ToListAsync();
            if (subjects == null || !subjects.Any())return new PagedResultDto<LookupDto>(0, new List<LookupDto>());
            var subjectDtos = _mapper.Map<List<LookupDto>>(subjects);
            return new PagedResultDto<LookupDto>(subjectDtos.Count, subjectDtos);
        }

        public async Task<PagedResultDto<LookupDto>> GetSubjectsWithCollegeMobListAsync(Guid collegeId, Guid? gradelevelId)
        {
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());

            // ✅ تأكد من أن collegeId موجود
            if (collegeId == Guid.Empty)
                return new PagedResultDto<LookupDto>(0, new List<LookupDto>());

            // ✅ 1. هات كل grade levels المرتبطة بالكلية
            var gradeLevelIds = await (await _gradeLevelRepository.GetQueryableAsync())
                .Where(x => x.CollegeId == collegeId)
                .Select(x => x.Id)
                .ToListAsync();

            if (!gradeLevelIds.Any())
                return new PagedResultDto<LookupDto>(0, new List<LookupDto>());

            // ✅ 2. بناء استعلام المواد
            var queryable = (await _subjectRepository.GetQueryableAsync())
                .Include(x => x.GradeLevel)
                .Where(s => gradeLevelIds.Contains((Guid)s.GradeLevelId));

            // ✅ 3. لو gradelevelId مش null فلتر عليها
            if (gradelevelId.HasValue)
                queryable = queryable.Where(s => s.GradeLevelId == gradelevelId.Value);

            // ✅ 4. تنفيذ الاستعلام وتحويله إلى DTO
            var subjects = await queryable.ToListAsync();
            if (!subjects.Any())
                return new PagedResultDto<LookupDto>(0, new List<LookupDto>());

            var subjectDtos = _mapper.Map<List<LookupDto>>(subjects);
            return new PagedResultDto<LookupDto>(subjectDtos.Count, subjectDtos);
        }

        public async Task<PagedResultDto<LookupDto>> GetSubjectsWithCollegeListAsync()
        {
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());
            var collegeId = currentUser.GetProperty<Guid?>(SetPropConsts.CollegeId);
            var gradeLevelIds = await (await _gradeLevelRepository.GetQueryableAsync())
                .Where(x => x.CollegeId == collegeId)
                .Select(x => x.Id)
                .ToListAsync();

            if (!gradeLevelIds.Any())
                return new PagedResultDto<LookupDto>(0, new List<LookupDto>());

            var queryable = (await _subjectRepository.GetQueryableAsync()).Include(x => x.GradeLevel)
                .Where(s => gradeLevelIds.Contains((Guid)s.GradeLevelId));

            var subjects = await queryable.ToListAsync();
            if (!subjects.Any()) return new PagedResultDto<LookupDto>(0, new List<LookupDto>());

            var subjectDtos = _mapper.Map<List<LookupDto>>(subjects);
            return new PagedResultDto<LookupDto>(subjectDtos.Count, subjectDtos);
        }
        public async Task<PagedResultDto<LookupDto>> GetSubjectsWithCollegeListAsync(Guid collegeId)
        {
            var gradeLevelIds = await (await _gradeLevelRepository.GetQueryableAsync()).Where(x => x.CollegeId == collegeId).Select(x => x.Id).ToListAsync();
            if (!gradeLevelIds.Any())  return new PagedResultDto<LookupDto>(0, new List<LookupDto>());
            var queryable = (await _subjectRepository.GetQueryableAsync()).Include(x => x.GradeLevel).Where(s => gradeLevelIds.Contains((Guid)s.GradeLevelId));
            var subjects = await queryable.ToListAsync();
            if (!subjects.Any()) return new PagedResultDto<LookupDto>(0, new List<LookupDto>());
            var subjectDtos = _mapper.Map<List<LookupDto>>(subjects);
            return new PagedResultDto<LookupDto>(subjectDtos.Count, subjectDtos);
        }

    }

}

