using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Courses
{
    public class CourseInfoAppService :ApplicationService
    {
        private readonly CourseInfoManager _courseinfoManager;
        public CourseInfoAppService(CourseInfoManager courseinfoManager)
        {
            _courseinfoManager = courseinfoManager;
        }
        [Authorize(AcadmyPermissions.CourseInfos.View)]
        public async Task<ResponseApi<CourseInfoDto>> GetAsync(Guid id) => await _courseinfoManager.GetAsync(id);
        [Authorize(AcadmyPermissions.CourseInfos.View)]
        public async Task<PagedResultDto<CourseInfoDto>> GetListAsync(int pageNumber, int pageSize, string? search, Guid courseId) => await _courseinfoManager.GetListAsync(pageNumber, pageSize, search,courseId);
        [Authorize(AcadmyPermissions.CourseInfos.Create)]
        public async Task<ResponseApi<CourseInfoDto>> CreateAsync(CreateUpdateCourseInfoDto input) => await _courseinfoManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.CourseInfos.Edit)]
        public async Task<ResponseApi<CourseInfoDto>> UpdateAsync(Guid id, CreateUpdateCourseInfoDto input) => await _courseinfoManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.CourseInfos.Delete)]
        public async Task DeleteAsync(Guid id) => await _courseinfoManager.DeleteAsync(id);
        [AllowAnonymous]
        public async Task<PagedResultDto<LookupDto>> GetCourseInfosListAsync() => await _courseinfoManager.GetCourseInfosListAsync();

    }
}
