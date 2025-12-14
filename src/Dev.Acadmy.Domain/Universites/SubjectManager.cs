using AutoMapper;
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
using Dev.Acadmy.LookUp;
using Volo.Abp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Dev.Acadmy.Courses;
using Dev.Acadmy.Dtos.Response.Subjects;
using Dev.Acadmy.Dtos.Response.Teachers;
using Dev.Acadmy.MediaItems;
using Volo.Abp.Application.Services;

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
        private readonly IRepository<Courses.Course> _courseRepository;
        private readonly IRepository<MediaItem, Guid> _mediaItemRepository;
        public SubjectManager(IRepository<MediaItem, Guid> mediaItemRepository, IRepository<Courses.Course> courseRepository, IRepository<GradeLevel, Guid> gradeLevelRepository, CourseManager courseManager, IIdentityUserRepository userRepository, IRepository<College, Guid> collegeRepository, ICurrentUser currentUser, IMapper mapper, IRepository<Subject , Guid> subjectRepository)
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



        public async Task<PagedResultDto<SubjectWithTeachersDto>> GetSubjectsWithTeachersAsync(
    int pageNumber = 1,
    int pageSize = 10,
    string? search = null)
        {
            // 1️⃣ جلب كل المواد
            var subjectsQuery = await _subjectRepository.GetQueryableAsync();

            // 1a️⃣ Apply search
            if (!string.IsNullOrWhiteSpace(search))
            {
                subjectsQuery = subjectsQuery
                    .Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = await subjectsQuery.CountAsync();

            // 2️⃣ Pagination
            var subjectsList = await subjectsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var subjectIds = subjectsList.Select(x => x.Id).ToList();

            // 3️⃣ جلب الكورسات المرتبطة بهذه المواد
            var coursesList = await (await _courseRepository.GetQueryableAsync())
                .Where(c => c.SubjectId != null && subjectIds.Contains(c.SubjectId.Value))
                .Include(c => c.User)
                .ToListAsync();

            // 4️⃣ جلب المدرسين
            var teacherIds = coursesList.Select(c => c.UserId).Distinct().ToList();
            var teachers = coursesList.Select(c => c.User).ToList();

            // 5️⃣ جلب الصور من MediaItems
            var mediaItems = await (await _mediaItemRepository.GetQueryableAsync())
                .Where(m => teacherIds.Contains(m.RefId))
                .ToListAsync();

            var mediaDict = mediaItems
                .GroupBy(m => m.RefId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault()?.Url);

            var random = new Random();

            // 6️⃣ Projection لكل مادة
            var result = subjectsList.Select(subject =>
            {
                var subjectTeachers = coursesList
                    .Where(c => c.SubjectId == subject.Id)
                    .Select(c => c.User)
                    .GroupBy(u => u.Id) // المدرس يظهر مرة واحدة لكل مادة
                    .Select(g => g.First())
                    .Select(t => new TeacherTopDto
                    {
                        Id = t.Id,
                        TeacherName = t.UserName,
                        TeacherImage = mediaDict.ContainsKey(t.Id) ? mediaDict[t.Id] : null,
                        SubjectName = subject.Name,
                        Rating = Math.Round(random.NextDouble() * 5, 1)
                    })
                    .ToList();

                return new SubjectWithTeachersDto
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    Teachers = subjectTeachers
                };
            }).ToList();

            // 7️⃣ إرجاع النتائج مع Pagination
            return new PagedResultDto<SubjectWithTeachersDto>(totalCount, result);
        }


    }

}

