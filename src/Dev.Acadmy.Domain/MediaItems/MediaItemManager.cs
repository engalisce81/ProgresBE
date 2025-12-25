using AutoMapper;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Dev.Acadmy.MediaItems
{
    public class MediaItemManager:DomainService
    {
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public MediaItemManager(IMediaItemRepository mediaItemRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _mediaItemRepository = mediaItemRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MediaItem?> GetAsync(Guid refId ) => await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == refId);

        public async Task<List<MediaItem>> GetListAsync(Guid refId) =>await (await _mediaItemRepository.GetQueryableAsync()).Where(x => x.RefId == refId).ToListAsync();

        public async Task<MediaItem> CreateAsync(CreateUpdateMediaItemDto input)
        {
            var mediaItem = _mapper.Map<MediaItem>(input);
            var result = await _mediaItemRepository.InsertAsync(mediaItem ,autoSave:true);
            return result;
        }

        public async Task CreateManyAsync(List<CreateUpdateMediaItemDto> inputs)
        {
            var mediaItems = _mapper.Map<List<MediaItem>>(inputs);
            foreach (var mediaItem in mediaItems) await _mediaItemRepository.InsertAsync(mediaItem ,autoSave:true);
        }

        public async Task<MediaItem> UpdateAsync(Guid id, CreateUpdateMediaItemDto input)
        {
            var mediaItemDB = await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == id);
            var result = new MediaItem();
            if (mediaItemDB == null) result = await CreateAsync(input);
            else
            {

                DeleteImageByUrlAsync(mediaItemDB.Url);
                var mediaItem = _mapper.Map(input, mediaItemDB);
                result = await _mediaItemRepository.UpdateAsync(mediaItem);
            }
            return result;
        }

        public async Task DeleteAsync(Guid id)
        {
            var mediaItem = await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == id);
            if (mediaItem != null)
            {
                DeleteImageByUrlAsync(mediaItem.Url);
                await _mediaItemRepository.DeleteAsync(mediaItem);
            }
        }

        private void DeleteImageByUrlAsync(string url)
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

        public async Task UpdateManyAsync(Guid id,List<CreateUpdateMediaItemDto> inputs)
        {
            var mediaItemsDB = await (await _mediaItemRepository.GetQueryableAsync()).Where(x => x.RefId == id).ToListAsync();
            var results = new List<MediaItem>();
            if (mediaItemsDB == null && mediaItemsDB.Count==0)
            {
                foreach (var input in inputs)
                {
                    var created = await CreateAsync(input);
                    results.Add(created);
                }
            }
            else
            {
                await DeleteManyAsync(id);
                var mediaItems = _mapper.Map(inputs, mediaItemsDB);
                await _mediaItemRepository.UpdateManyAsync(mediaItems);
            } 
        }

        public async Task DeleteManyAsync(Guid id)
        {
            var mediaItems = await (await _mediaItemRepository.GetQueryableAsync()).Where(x => x.RefId == id).ToListAsync();
            foreach(var mediaItem in mediaItems)
            {
                DeleteImageByUrlAsync(mediaItem.Url);
                await _mediaItemRepository.DeleteAsync(mediaItem,autoSave:true);
            }
        }

        public async Task<PagedResultDto<string>> UploadImagesAsync(ICollection<IFormFile> files)
        {
            var urls = new List<string>();
            foreach (var file in files) 
            {
                var image =await UploadImageAsync(file);
                urls.Add(image.Data);
            }
            return new PagedResultDto<string>(urls.Count, urls);
        }


        public async Task<ResponseApi<string>> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return new ResponseApi<string> { Data = null, Message = "file not found", Success = false };
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
            return new ResponseApi<string> { Data = fileUrl, Message = "save success", Success = true };
        }

        public async Task<string> UpdateAsync(IFormFile file, Guid refId)
        {
            string url = string.Empty;
            if (file != null)
            {
                // ميثود InsertAsync هنا يجب أن تتعامل مع استبدال الصورة القديمة بالجديدة
                url = await _mediaItemRepository.UpdateAsync(file, refId );
            }
            else
            {
                // إذا لم يتم رفع ملف جديد، نجلب الرابط الحالي للحفاظ عليه في الـ DTO
                url = (await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == refId))?.Url ?? string.Empty;
            }
            return url;
        }
    }
}

