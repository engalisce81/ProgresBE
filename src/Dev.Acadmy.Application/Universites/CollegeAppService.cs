
using Dev.Acadmy.LookUp;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Universites
{
    public class CollegeAppService :ApplicationService
    {
        private readonly CollegeManager _collegeManager;
        public CollegeAppService(CollegeManager collegeManager) 
        {
            _collegeManager = collegeManager;
        }
        [Authorize(AcadmyPermissions.Colleges.View)]
        public async Task<ResponseApi<CollegeDto>> GetAsync(Guid id) => await _collegeManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Colleges.View)]
        public async Task<PagedResultDto<CollegeDto>> GetListAsync(int pageNumber, int pageSize, string? search, Guid universityId) => await _collegeManager.GetListAsync(pageNumber, pageSize, search , universityId);
        [Authorize(AcadmyPermissions.Colleges.Create)]
        public async Task<ResponseApi<CollegeDto>> CreateAsync(CreateUpdateCollegeDto input) => await _collegeManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Colleges.Edit)]
        public async Task<ResponseApi<CollegeDto>> UpdateAsync(Guid id, CreateUpdateCollegeDto input) => await _collegeManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Colleges.Delete)]
        public async Task DeleteAsync(Guid id) => await _collegeManager.DeleteAsync(id);
        [AllowAnonymous]
        public async Task<PagedResultDto<LookupDto>> GetCollegesListAsync(Guid universityId) => await _collegeManager.GetCollegesListAsync(universityId);
        [AllowAnonymous]
        public async Task<PagedResultDto<LookupDto>> GetGradeLevelListAsync(Guid collegeId) => await _collegeManager.GetGradeLevelListAsync(collegeId);
        [AllowAnonymous]
        public async Task<PagedResultDto<LookupDto>> GetTermListAsync() => await _collegeManager.GetTermListAsync();

    }
}
