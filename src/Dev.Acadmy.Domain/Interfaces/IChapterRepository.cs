using Dev.Acadmy.Chapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IChapterRepository :IRepository<Chapter, Guid>
    {
        Task<(List<Chapter> Items, int TotalCount)> GetPagedChaptersWithDetailsAsync(Guid courseId, int skipCount, int maxResultCount);
    }
}
