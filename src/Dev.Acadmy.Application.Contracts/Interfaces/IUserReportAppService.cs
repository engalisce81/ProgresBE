using System;
using System.Threading.Tasks;
using Dev.Acadmy.Dtos.Request.Reports;
using Dev.Acadmy.Dtos.Response.Reports;
using Dev.Acadmy.Response;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Interfaces
{
    public interface IUserReportAppService : IApplicationService
    {
        // --- عمليات المستخدم العادي ---

        Task<ResponseApi<ReportDto>> CreateAsync(CreateUpdateReportDto input);

        Task<ResponseApi<ReportDto>> UpdateAsync(Guid id, CreateUpdateReportDto input);

        // جلب بلاغات المستخدم الحالي مع Pagination
        Task<PagedResultDto<ReportDto>> GetMyReportsAsync(int pageNumber, int pageSize);


        // --- عمليات المشرفين (Admin) ---

        // جلب كل البلاغات مع Pagination وفلترة
        Task<PagedResultDto<ReportDto>> GetListAsync(int? type, int? status, int pageNumber, int pageSize);

        Task<ResponseApi<ReportDto>> GetAsync(Guid id);

        Task<ResponseApi<ReportDto>> UpdateStatusAsync(Guid id, int status);

        Task DeleteAsync(Guid id);
    }
}