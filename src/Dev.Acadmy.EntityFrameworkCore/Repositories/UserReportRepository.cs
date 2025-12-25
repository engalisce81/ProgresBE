using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Entities.Reports;
using Dev.Acadmy.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Dev.Acadmy.Entities.Reports.Entities;
using Dev.Acadmy.Interfaces;

namespace Dev.Acadmy.Reports
{
    public class UserReportRepository : EfCoreRepository<AcadmyDbContext, UserReport, Guid>, IUserReportRepository
    {
        public UserReportRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<(List<UserReport> Items, int TotalCount)> GetListAsync(
            ReportType? type,
            ReportStatus? status,
            int skipCount,
            int maxResultCount)
        {
            var query = await GetDbSetAsync();

            // Apply Filters
            var filteredQuery = query
                .WhereIf(type.HasValue, x => x.Type == type)
                .WhereIf(status.HasValue, x => x.Status == status);

            var totalCount = await filteredQuery.CountAsync();

            var items = await filteredQuery
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<UserReport> Items, int TotalCount)> GetListByUserIdAsync(
            Guid userId,
            int skipCount,
            int maxResultCount)
        {
            var query = await GetDbSetAsync();

            var filteredQuery = query.Where(x => x.UserId == userId);

            var totalCount = await filteredQuery.CountAsync();

            var items = await filteredQuery
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}