using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.Lectures;
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
    public class CourseAppService :ApplicationService
    {
        private readonly CourseManager _courseManager;
        public CourseAppService(CourseManager courseManager)
        {
            _courseManager = courseManager;
        }
        [Authorize(AcadmyPermissions.Courses.View)]
        public async Task<ResponseApi<CourseDto>> GetAsync(Guid id) => await _courseManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Courses.View)]
        public async Task<PagedResultDto<CourseDto>> GetListAsync(int pageNumber, int pageSize, string? search) => await _courseManager.GetListAsync(pageNumber, pageSize, search);
        [Authorize(AcadmyPermissions.Courses.Create)]
        public async Task<ResponseApi<CourseDto>> CreateAsync(CreateUpdateCourseDto input) => await _courseManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Courses.Edit)]
        public async Task<ResponseApi<CourseDto>> UpdateAsync(Guid id, CreateUpdateCourseDto input) => await _courseManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Courses.Delete)]
        public async Task DeleteAsync(Guid id) => await _courseManager.DeleteAsync(id);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetCoursesListAsync() => await _courseManager.GetCoursesListAsync();
        [Authorize]
        public async Task<PagedResultDto<CourseInfoHomeDto>> GetCoursesInfoListAsync(int pageNumber, int pageSize, string? search ,bool alreadyJoin,Guid collegeId, Guid? subjectId, Guid? gradelevelId) => await _courseManager.GetCoursesInfoListAsync(pageNumber, pageSize, search,alreadyJoin,collegeId, subjectId,  gradelevelId);
        [Authorize]
        public async Task<ResponseApi<CourseInfoHomeDto>> GetCoursesInfoAsync(Guid courseId) => await _courseManager.GetCoursesInfoAsync(courseId);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetMyCoursesLookUpAsync() => await _courseManager.GetMyCoursesLookUpAsync();
        [Authorize]
        public async Task<Guid> DuplicateCourseAsync(Guid courseId) => await _courseManager.DuplicateCourseAsync(courseId);
        [Authorize]
        public async Task<PagedResultDto<LectureWithQuizzesDto>> GetStudentQuizzesByCourseAsync(Guid courseId, Guid userId, int pageNumber, int pageSize)=> await _courseManager.GetStudentQuizzesByCourseAsync(courseId, userId, pageNumber, pageSize);
    }
}
