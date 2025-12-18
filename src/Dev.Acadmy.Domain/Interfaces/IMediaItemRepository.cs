using Dev.Acadmy.MediaItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IMediaItemRepository : IRepository<MediaItem, Guid>
    {
        Task<Dictionary<Guid, string>> GetUrlDictionaryByRefIdsAsync(List<Guid> refIds);
    }
}
