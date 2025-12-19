using Dev.Acadmy.LookUp;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Chapters
{
    public class ChapterAppService : ApplicationService
    {
        private readonly ChapterManager _chapterManager;
        public ChapterAppService(ChapterManager chapterManager)
        {
            _chapterManager = chapterManager;
        }
        [Authorize(AcadmyPermissions.Chapters.View)]
        public async Task<ResponseApi<ChapterDto>> GetAsync(Guid id) => await _chapterManager.GetAsync(id);
        [Authorize(AcadmyPermissions.Chapters.View)]
        public async Task<PagedResultDto<ChapterDto>> GetListAsync(int pageNumber, int pageSize, string? search,Guid courseId) => await _chapterManager.GetListAsync(pageNumber, pageSize, search,courseId);
        [Authorize(AcadmyPermissions.Chapters.Create)]
        public async Task<ResponseApi<ChapterDto>> CreateAsync(CreateUpdateChapterDto input) => await _chapterManager.CreateAsync(input);
        [Authorize(AcadmyPermissions.Chapters.Edit)]
        public async Task<ResponseApi<ChapterDto>> UpdateAsync(Guid id, CreateUpdateChapterDto input) => await _chapterManager.UpdateAsync(id, input);
        [Authorize(AcadmyPermissions.Chapters.Delete)]
        public async Task DeleteAsync(Guid id) => await _chapterManager.DeleteAsync(id);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetListChaptersAsync() => await _chapterManager.GetListChaptersAsync();
        [Authorize]
        public async Task<PagedResultDto<CourseChaptersDto>> GetCourseChaptersAsync(Guid courseId, int pageNumber, int pageSize) => await _chapterManager.GetCourseChaptersAsync(courseId, pageNumber, pageSize);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetChaptersByCourseLookUpAsync(Guid courseId) => await _chapterManager.GetChaptersByCourseLookUpAsync(courseId);
    }
}
