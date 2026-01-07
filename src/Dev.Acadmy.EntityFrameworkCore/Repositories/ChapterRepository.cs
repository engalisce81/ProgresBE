using Dev.Acadmy.Chapters;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class ChapterRepository : EfCoreRepository<AcadmyDbContext, Chapter, Guid>, IChapterRepository
    {
        public ChapterRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider) { }

        public async Task<(List<Chapter> Items, int TotalCount)> GetPagedChaptersWithDetailsAsync(
           Guid courseId,
           int skipCount,
           int maxResultCount)
        {
            var query = (await GetQueryableAsync())
                .Include(x => x.Course)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quizzes)
                        .ThenInclude(q => q.Questions)
                .Where(c => c.CourseId == courseId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
