using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Teachers
{
    public class TeacherAppService :ApplicationService
    {
        private readonly TeacherManager _teacherManager;
        public TeacherAppService(TeacherManager teacherManager)
        {
            _teacherManager = teacherManager;
        }
        [Authorize(AcadmyPermissions.Teachers.Create)]
        public async Task CreateAsync(CreateUpdateTeacherDto input) => await _teacherManager.CreateTeacherAsync(input);
        [Authorize(AcadmyPermissions.Teachers.Edit)]

        public async Task UpdateAsync(Guid id, CreateUpdateTeacherDto input) => await _teacherManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Teachers.Delete)]

        public async Task DeleteAsync(Guid id) => await _teacherManager.DeleteAsync(id);
        [Authorize(AcadmyPermissions.Teachers.View)]
        public async Task<PagedResultDto<TeacherDto>> GetTeacherListAsync(int pageNumber, int pageSize, string? search = null) => await _teacherManager.GetTeacherListAsync(pageNumber, pageSize, search);
        [Authorize(AcadmyPermissions.Teachers.View)]
        public async Task<ResponseApi<TeacherDto>> GetAsync(Guid userId) => await _teacherManager.GetAsync(userId);

    }
}
