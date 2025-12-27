using Dev.Acadmy.Entities.Advertisementes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;


namespace Dev.Acadmy.Entities.Advertisementes.Managers
{
    
    public class AdvertisementManager : DomainService
    {
        private readonly IRepository<Advertisement, Guid> _adRepository;

        public AdvertisementManager(IRepository<Advertisement, Guid> adRepository)
        {
            _adRepository = adRepository;
        }

        // Get Single Entity
        public async Task<Advertisement> GetAsync(Guid id)
        {
            var ad = await _adRepository.GetAsync(id);
            if (ad == null) throw new UserFriendlyException("الإعلان غير موجود");
            return ad;
        }

        // Get List with Logic
        public async Task<(List<Advertisement> items, int totalCount)> GetListAsync(
            int pageNumber, int pageSize, string? search)
        {
            var queryable = await _adRepository.GetQueryableAsync();

            queryable = queryable.WhereIf(!string.IsNullOrWhiteSpace(search),
                x => x.Title.Contains(search!) );

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var skipCount = (pageNumber - 1) * pageSize;

            var ads = await AsyncExecuter.ToListAsync(
                queryable.OrderByDescending(x => x.CreationTime)
                         .Skip(skipCount).Take(pageSize));

            return (ads, totalCount);
        }

        // Create Logic
        public async Task<Advertisement> CreateAsync(
            string title,
            string youtubeUrl,
            string driveUrl,
            DateTime startDate,
            DateTime endDate,
            bool isActive)
        {
            // 1. منطق عمل: التأكد أن تاريخ النهاية بعد البداية
            if (endDate <= startDate)
            {
                throw new UserFriendlyException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء");
            }

            // 2. منطق عمل إضافي: يمكنك هنا التأكد مثلاً من عدم تكرار العنوان
            var existing = await _adRepository.FirstOrDefaultAsync(x => x.Title == title);
            if (existing != null)
            {
                throw new UserFriendlyException("يوجد إعلان بنفس هذا العنوان بالفعل");
            }

            return new Advertisement( title,youtubeUrl ,driveUrl,startDate , endDate , isActive);
        }

        // Update Logic
        public async Task UpdateAsync(
        Advertisement ad, // الكيان الحالي المراد تعديله
        string title, string imageUrl, string youTubeVideoUrl,string driveVideoUrl,
        DateTime startDate, DateTime endDate, bool isActive)
        {
            // 1. التأكد أن تاريخ النهاية بعد البداية
            if (endDate <= startDate)
            {
                throw new UserFriendlyException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البدء");
            }

            // 2. التأكد من عدم تكرار العنوان مع إعلان آخر (غير الحالي)
            var existing = await _adRepository.FirstOrDefaultAsync(x => x.Title == title && x.Id != ad.Id);
            if (existing != null)
            {
                throw new UserFriendlyException("يوجد إعلان آخر بنفس هذا العنوان بالفعل");
            }

            // 3. تحديث البيانات
            ad.Title = title;
            ad.YouTubeVideoUrl = youTubeVideoUrl;
            ad.DriveVideoUrl = driveVideoUrl;
            ad.StartDate = startDate;
            ad.EndDate = endDate;
            ad.IsActive = isActive;
        }

        // Delete Logic
        public async Task DeleteAsync(Guid id)
        {
            await _adRepository.DeleteAsync(id);
        }
    }
}
