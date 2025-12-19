using Dev.Acadmy.Dtos.Request.Advertisementes;
using Dev.Acadmy.Dtos.Response.Advertisementes;
using Dev.Acadmy.Response;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Interfaces
{
    public interface IAdvertisementAppService : IApplicationService
    {
        // الحصول على إعلان واحد
        Task<ResponseApi<AdvertisementDto>> GetAsync(Guid id);

        // الحصول على قائمة الإعلانات (للمدير)
        Task<PagedResultDto<AdvertisementDto>> GetListAsync(int pageNumber = 1,int pageSize = 10,string? search = null);

        // إضافة إعلان جديد - ترجع نجاح أو فشل العملية
        Task<ResponseApi<bool>> CreateAsync(CreateUpdateAdvertisementDto input);

        // تحديث إعلان - ترجع نجاح أو فشل العملية
        Task<ResponseApi<bool>> UpdateAsync(Guid id, CreateUpdateAdvertisementDto input);

        // حذف إعلان
        Task<ResponseApi<bool>> DeleteAsync(Guid id);

        // الميثود الخاصة بالموبايل (الإعلان النشط حالياً)
        Task<PagedResultDto<AdvertisementDto>> GetActiveAdsListAsync(int pageNumber = 1, int pageSize = 10);
    }
}