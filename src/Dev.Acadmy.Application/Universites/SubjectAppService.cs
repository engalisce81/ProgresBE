using Dev.Acadmy.Dtos.Response.Subjects;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Universites
{
    public class SubjectAppService :ApplicationService
    {
        private readonly SubjectManager _subjectManager;
        public SubjectAppService(SubjectManager subjectManager)
        {
            _subjectManager = subjectManager;
        }
        [Authorize(AcadmyPermissions.Subjects.View)]
        public async Task<ResponseApi<SubjectDto>> GetAsync(Guid id) => await _subjectManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Subjects.View)]
        public async Task<PagedResultDto<SubjectDto>> GetListAsync(int pageNumber, int pageSize, string? search) => await _subjectManager.GetListAsync(pageNumber, pageSize, search);
        [Authorize(AcadmyPermissions.Subjects.Create)]
        public async Task<ResponseApi<SubjectDto>> CreateAsync(CreateUpdateSubjectDto input) => await _subjectManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Subjects.Edit)]
        public async Task<ResponseApi<SubjectDto>> UpdateAsync(Guid id, CreateUpdateSubjectDto input) => await _subjectManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Subjects.Delete)]
        public async Task DeleteAsync(Guid id) => await _subjectManager.DeleteAsync(id);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetSubjectsListAsync() => await _subjectManager.GetSubjectsListAsync();
       
        // to admin panel
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetSubjectsWithCollegeMobListAsync(Guid collegeId, Guid? gradelevelId) => await _subjectManager.GetSubjectsWithCollegeMobListAsync(collegeId,  gradelevelId);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetSubjectsWithCollegeListAsync()=> await _subjectManager.GetSubjectsWithCollegeListAsync();
        [AllowAnonymous]
        public async Task<PagedResultDto<SubjectWithTeachersDto>> GetSubjectsWithTeachersAsync(int pageNumber = 1,int pageSize = 10, string? search = null) => await _subjectManager.GetSubjectsWithTeachersAsync(pageNumber, pageSize,search);
    }
}
