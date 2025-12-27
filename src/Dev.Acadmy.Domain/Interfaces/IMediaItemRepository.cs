using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Http;
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
        Task<string> InsertAsync(IFormFile file , Guid RefId );
        Task<string> UpdateAsync(IFormFile file, Guid RefId);
        Task<string> GetUrlByEntityIdAsync(Guid refId);


    }
}
