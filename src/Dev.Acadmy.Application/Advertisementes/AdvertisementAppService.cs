using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Dev.Acadmy.Entities.Advertisementes.Entities;
using Dev.Acadmy.Entities.Advertisementes.Managers;
using Dev.Acadmy.Interfaces; // تأكد من وجود الـ Interface هنا
using Dev.Acadmy.Response;
using Dev.Acadmy.Dtos.Response.Advertisementes;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Dev.Acadmy.Dtos.Request.Advertisementes;
using System.Linq;
using Dev.Acadmy.MediaItems;
using Microsoft.AspNetCore.Authorization;
using Dev.Acadmy.Permissions;

namespace Dev.Acadmy.Services
{
    public class AdvertisementAppService : AcadmyAppService, IAdvertisementAppService
    {
        private readonly AdvertisementManager _adManager;
        private readonly IRepository<Advertisement, Guid> _adRepository;
        private readonly IMapper _mapper;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly MediaItemManager _mediaItemManager;
        public AdvertisementAppService(
            AdvertisementManager adManager,
            IRepository<Advertisement, Guid> adRepository,
            IMediaItemRepository mediaItemRepository,
            IMapper mapper,
            MediaItemManager mediaItemManager)
        {
            _mediaItemManager = mediaItemManager;
            _mediaItemRepository = mediaItemRepository;
            _adManager = adManager;
            _adRepository = adRepository;
            _mapper = mapper;
        }

        // 1. جلب إعلان واحد بالتفصيل
        [Authorize(AcadmyPermissions.Advertisements.View)]
        public async Task<ResponseApi<AdvertisementDto>> GetAsync(Guid id)
        {
            var ad = await _adManager.GetAsync(id);
            var dto = _mapper.Map<AdvertisementDto>(ad);
            var mediaItem = await _mediaItemRepository.FirstOrDefaultAsync(x=>x.RefId == id);
            if (mediaItem != null)
            {
                dto.ImageUrl = mediaItem.Url;
            }
            return new ResponseApi<AdvertisementDto>()
            {
                Data = dto,
                Success = true,
                Message = "تم جلب بيانات الإعلان بنجاح"
            };
        }

        // 2. جلب القائمة (للمدير) مع البحث والتقسيم
        [Authorize(AcadmyPermissions.Advertisements.View)]
        public async Task<PagedResultDto<AdvertisementDto>> GetListAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var (items, totalCount) = await _adManager.GetListAsync(pageNumber, pageSize, search);
            var dtos = _mapper.Map<List<AdvertisementDto>>(items);
            var refIds = dtos.Select(x=>x.Id).ToList();
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(refIds);
            foreach (var dto in dtos)
            {
                if (mediaItemDic.TryGetValue(dto.Id, out var url))
                {
                    dto.ImageUrl = url;
                }
            }

            return new PagedResultDto<AdvertisementDto>(totalCount, dtos);
        }

        // 3. إضافة إعلان جديد
        [Authorize(AcadmyPermissions.Advertisements.Create)]
        public async Task<ResponseApi<bool>> CreateAsync(CreateUpdateAdvertisementDto input)
        {
            var ad = await _adManager.CreateAsync(
                input.Title,
                input.TargetUrl,
                input.StartDate,
                input.EndDate,
                input.IsActive
            );
            await _mediaItemRepository.InsertAsync(new MediaItem {RefId = ad.Id , IsImage =true , Url = input.ImageUrl});
            await _adRepository.InsertAsync(ad);

            return new ResponseApi<bool>()
            {
                Data = true,
                Success = true,
                Message = "تم إنشاء الإعلان بنجاح"
            };
        }

        // 4. تحديث إعلان موجود
        [Authorize(AcadmyPermissions.Advertisements.Edit)]

        public async Task<ResponseApi<bool>> UpdateAsync(Guid id, CreateUpdateAdvertisementDto input)
        {
            var ad = await _adRepository.GetAsync(id);
            await _adManager.UpdateAsync(
                ad,
                input.Title,
                input.ImageUrl,
                input.TargetUrl,
                input.StartDate,
                input.EndDate,
                input.IsActive
            );
            await _adRepository.UpdateAsync(ad);
            await _mediaItemManager.UpdateAsync(id, new CreateUpdateMediaItemDto { RefId = id, IsImage = true, Url = input.ImageUrl });
            return new ResponseApi<bool>()
            {
                Data = true,
                Success = true,
                Message = "تم تحديث الإعلان بنجاح"
            };
        }

        // 5. حذف إعلان
        [Authorize(AcadmyPermissions.Advertisements.Delete)]
        public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
        {
            await _adManager.DeleteAsync(id);
            return new ResponseApi<bool>()
            {
                Data = true,
                Success = true,
                Message = "تم حذف الإعلان بنجاح"
            };
        }

        [Authorize(AcadmyPermissions.Advertisements.Default)]

        // 6. ميثود الموبايل (الإعلان النشط حالياً)
        public async Task<ResponseApi<AdvertisementDto>> GetActiveAdAsync()
        {
            var now = Clock.Now;
            var queryable = await _adRepository.GetQueryableAsync();

            // بحث عن أول إعلان نشط يقع ضمن النطاق الزمني
            var ad = await AsyncExecuter.FirstOrDefaultAsync(queryable,
                x => x.IsActive && x.StartDate <= now && x.EndDate >= now);

            if (ad == null)
            {
                return new ResponseApi<AdvertisementDto>()
                {
                    Data = null,
                    Success = false,
                    Message = "لا يوجد إعلان نشط حالياً"
                };
            }

            var dto = _mapper.Map<AdvertisementDto>(ad);
            return new ResponseApi<AdvertisementDto>()
            {
                Data = dto,
                Success = true,
                Message = "تم جلب الإعلان النشط"
            };
        }

        // 6. جلب قائمة الإعلانات النشطة مع Pagination
        public async Task<PagedResultDto<AdvertisementDto>> GetActiveAdsListAsync(int pageNumber = 1, int pageSize = 10)
        {
            var now = Clock.Now;
            var skipCount = (pageNumber - 1) * pageSize;
            var queryable = await _adRepository.GetQueryableAsync();

            // 1. فلترة الإعلانات النشطة حالياً
            var query = queryable.Where(x => x.IsActive && x.StartDate <= now && x.EndDate >= now);

            // 2. حساب العدد الإجمالي قبل التقسيم لصفحات
            var totalCount = await AsyncExecuter.CountAsync(query);

            // 3. جلب البيانات المرتبة (الأحدث أولاً) مع Skip و Take
            var ads = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(x => x.CreationTime)
                     .Skip(skipCount)
                     .Take(pageSize)
            );

            // 4. جلب الصور الخاصة بهذه الإعلانات دفعة واحدة (Batch Loading)
            // نفترض أن الـ Advertisement يستخدم الـ Id الخاص به كـ RefId في جدول الميديا
            var adIds = ads.Select(x => x.Id).ToList();
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(adIds);

            // 5. تحويل البيانات إلى DTO مع دمج روابط الصور كاملة
            var dtos = ads.Select(ad =>
            {
                var dto = _mapper.Map<AdvertisementDto>(ad);
                // الحصول على الرابط من القاموس وتحويله لرابط كامل باستخدام ميثود GetFullUrl (الموجودة في الخدمة)
                var imageUrl = mediaItemDic.ContainsKey(ad.Id) ? mediaItemDic[ad.Id] : string.Empty;
                dto.ImageUrl = imageUrl;
                return dto;
            }).ToList();

            return new PagedResultDto<AdvertisementDto>(totalCount, dtos);
        }
    }
}