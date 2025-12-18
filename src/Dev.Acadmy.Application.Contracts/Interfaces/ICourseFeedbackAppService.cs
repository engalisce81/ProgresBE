using Dev.Acadmy.Dtos.Request.Courses;
using Dev.Acadmy.Dtos.Response.Courses;
using Dev.Acadmy.Response;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseFeedbackAppService : IApplicationService
    {
        Task<ResponseApi<bool>> CreateAsync(CreateUpdateCourseFeedbackDto input);

        Task<ResponseApi<bool>> UpdateAsync(Guid id, CreateUpdateCourseFeedbackDto input);

        Task<ResponseApi<bool>> DeleteAsync(Guid id);

        Task<ResponseApi<bool>> AcceptFeedbackAsync(Guid id, bool isAccept);
    }
}
