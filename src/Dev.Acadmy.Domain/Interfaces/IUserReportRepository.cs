using Dev.Acadmy.Entities.Reports.Entities;
using Dev.Acadmy.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IUserReportRepository : IRepository<UserReport, Guid>
    {
        // Custom method for Admin to filter by Type and Status
        Task<(List<UserReport> Items, int TotalCount)> GetListAsync(
            ReportType? type,
            ReportStatus? status,
            int skipCount,
            int maxResultCount);

        // Custom method to fetch reports for a specific user
        Task<(List<UserReport> Items, int TotalCount)> GetListByUserIdAsync(
            Guid userId,
            int skipCount,
            int maxResultCount);
    }
}
