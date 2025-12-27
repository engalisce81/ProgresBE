using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.MediaItems;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class MediaItemRepository
        : EfCoreRepository<AcadmyDbContext, MediaItem, Guid>, IMediaItemRepository
    {
        public MediaItemRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<Dictionary<Guid, string>> GetUrlDictionaryByRefIdsAsync(List<Guid> refIds)
        {
            var dbSet = await GetDbSetAsync();

            var query = await dbSet
                .Where(x => refIds.Contains(x.RefId))
                .GroupBy(x => x.RefId) // تجميع الصور لكل مستخدم/كيان
                .Select(group => new
                {
                    EntityId = group.Key,
                    // نأخذ أحدث صورة تم رفعها بناءً على الـ CreationTime أو الـ ID
                    Url = group.OrderByDescending(x => x.CreationTime).Select(x => x.Url).FirstOrDefault()
                })
                .ToListAsync();

            // الآن نحول القائمة النظيفة (بدون تكرار) إلى Dictionary
            return query.ToDictionary(x => x.EntityId, x => x.Url);
        }
    }
}
