using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.MediaItems;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class MediaItemRepository
        : EfCoreRepository<AcadmyDbContext, MediaItem, Guid>, IMediaItemRepository
    {
       
        private readonly IHttpContextAccessor _httpContextAccessor;

           public MediaItemRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider , IHttpContextAccessor httpContextAccessor)  : base(dbContextProvider)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async  Task<string> GetUrlByEntityIdAsync(Guid refId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(x => x.RefId == refId)
                .OrderByDescending(x => x.CreationTime)
                .Select(x => x.Url)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<Guid, string>> GetUrlDictionaryByRefIdsAsync(List<Guid> refIds)
        {
            // 1. حماية ضد القوائم الفارغة
            if (refIds == null || !refIds.Any())
                return new Dictionary<Guid, string>();

            var dbSet = await GetDbSetAsync();

            // 2. جلب البيانات الأساسية أولاً (Materialize)
            var rawData = await dbSet
                .Where(x => refIds.Contains(x.RefId))
                .Select(x => new { x.RefId, x.Url, x.CreationTime })
                .ToListAsync();

            // 3. المعالجة في الذاكرة (In-Memory) لتجنب تعقيدات SQL GroupBy
            return rawData
                .GroupBy(x => x.RefId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreationTime).Select(x => x.Url).FirstOrDefault() ?? ""
                );
        }

        public async Task<string> InsertAsync(IFormFile file, Guid RefId)
        {
            var fileUrl = await AddFormFileToImageFolder(file);
            var dbSet = await GetDbSetAsync();
            await dbSet.AddAsync(new MediaItem { RefId = RefId, IsImage = true, Url = fileUrl });
            return fileUrl;
        }

        public async Task<string> UpdateAsync(IFormFile file, Guid RefId )
        {
            var dbSet = await GetDbSetAsync();
            var mediaItem = await dbSet.FirstOrDefaultAsync(x => x.RefId == RefId);
            if (mediaItem != null)
            {
                DeleteFormFileByUrlFromImageFolderAsync(mediaItem.Url);
                var fileUrl = await AddFormFileToImageFolder(file);
                mediaItem.Url = fileUrl;
                await UpdateAsync(mediaItem, autoSave: true);
                return fileUrl;
            }
            else
            {
                return await InsertAsync(file, RefId);
            }
        }


        private async Task<string> AddFormFileToImageFolder(IFormFile file)
        {
            // تأكد من وجود فولدر images
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);
            // اسم الملف
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(imagesPath, fileName);
            // حفظ الملف
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // جلب BaseUrl
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            // URL كامل للملف
            var fileUrl = $"{baseUrl}/images/{fileName}";
            return fileUrl;
        }

        private void DeleteFormFileByUrlFromImageFolderAsync(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return; // مفيش URL
                }

                // لازم يكون URL يبدأ بدومين المشروع
                if (!url.StartsWith("https://scola-dev-be.demo.egisg.com"))
                {
                    return; // مش صورة عندنا
                }

                var relativePath = new Uri(url).AbsolutePath;
                if (relativePath.StartsWith("/"))
                    relativePath = relativePath.Substring(1);

                var wwwRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                var filePath = Path.Combine(wwwRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // هنا تسجل اللوج بس، ما توقفش التعديل كله
                Console.WriteLine($"DeleteImageByUrl Error: {ex.Message}");
            }
        }
    }
}
