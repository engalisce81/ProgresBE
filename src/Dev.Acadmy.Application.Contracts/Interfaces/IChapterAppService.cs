using Dev.Acadmy.Chapters;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Interfaces
{
    public interface IChapterAppService : IApplicationService
    {
        Task<PagedResultDto<CourseChaptersDto>> GetCourseChaptersAsync(Guid courseId, int pageNumber, int pageSize);

    }
}
