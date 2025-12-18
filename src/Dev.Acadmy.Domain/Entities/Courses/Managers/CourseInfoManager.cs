using AutoMapper;
using Dev.Acadmy.Courses;
using Dev.Acadmy.LookUp;
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

namespace Dev.Acadmy.Entities.Courses.Managers
{
    public class CourseInfoManager:DomainService
    {
        private readonly IRepository<CourseInfo, Guid> _courseinfoRepository;
        private readonly IMapper _mapper;
        public CourseInfoManager(IMapper mapper, IRepository<CourseInfo, Guid> courseinfoRepository)
        {
            _courseinfoRepository = courseinfoRepository;
            _mapper = mapper;
        }

        public async Task<ResponseApi<CourseInfoDto>> GetAsync(Guid id)
        {
            var courseinfo = await _courseinfoRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (courseinfo == null) return new ResponseApi<CourseInfoDto> { Data = null, Success = false, Message = "Not found courseinfo" };
            var dto = _mapper.Map<CourseInfoDto>(courseinfo);
            return new ResponseApi<CourseInfoDto> { Data = dto, Success = true, Message = "find succeess" };
        }

        public async Task<PagedResultDto<CourseInfoDto>> GetListAsync(int pageNumber, int pageSize, string? search,Guid courseId)
        {
            var queryable = await _courseinfoRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(search)) queryable = queryable.Where(c => c.Name.Contains(search));
            queryable = queryable.Where(x => x.CourseId == courseId);
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var courseinfos = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime).Skip((pageNumber - 1) * pageSize).Take(pageSize));
            var courseinfoDtos = _mapper.Map<List<CourseInfoDto>>(courseinfos);
            return new PagedResultDto<CourseInfoDto>(totalCount, courseinfoDtos);
        }

        public async Task<ResponseApi<CourseInfoDto>> CreateAsync(CreateUpdateCourseInfoDto input)
        {
            var courseinfo = _mapper.Map<CourseInfo>(input);
            var result = await _courseinfoRepository.InsertAsync(courseinfo,autoSave:true);
            var dto = _mapper.Map<CourseInfoDto>(result);
            return new ResponseApi<CourseInfoDto> { Data = dto, Success = true, Message = "save succeess" };
        }

        public async Task<ResponseApi<CourseInfoDto>> UpdateAsync(Guid id, CreateUpdateCourseInfoDto input)
        {
            var courseinfoDB = await _courseinfoRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (courseinfoDB == null) return new ResponseApi<CourseInfoDto> { Data = null, Success = false, Message = "Not found courseinfo" };
            var courseinfo = _mapper.Map(input, courseinfoDB);
            var result = await _courseinfoRepository.UpdateAsync(courseinfo);
            var dto = _mapper.Map<CourseInfoDto>(result);
            return new ResponseApi<CourseInfoDto> { Data = dto, Success = true, Message = "update succeess" };
        }

        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            var courseinfo = await _courseinfoRepository.FirstOrDefaultAsync(x => x.Id == id);
            if (courseinfo == null) return new ResponseApi<bool> { Data = false, Success = false, Message = "Not found courseinfo" };
            await _courseinfoRepository.DeleteAsync(courseinfo);
            return new ResponseApi<bool> { Data = true, Success = true, Message = "delete succeess" };
        }

        public async Task<PagedResultDto<LookupDto>> GetCourseInfosListAsync()
        {
            var queryable = await _courseinfoRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var courseinfos = await AsyncExecuter.ToListAsync(queryable.OrderByDescending(c => c.CreationTime));
            var courseinfoDtos = _mapper.Map<List<LookupDto>>(courseinfos);
            return new PagedResultDto<LookupDto>(totalCount, courseinfoDtos);
        }

        public async Task DeleteCourseInfoByCourseId(Guid courseId)
        {
            var infos = await (await _courseinfoRepository.GetQueryableAsync()).Where(x=>x.CourseId == courseId).ToListAsync();
            await _courseinfoRepository.DeleteManyAsync(infos);
        }
    }
}
