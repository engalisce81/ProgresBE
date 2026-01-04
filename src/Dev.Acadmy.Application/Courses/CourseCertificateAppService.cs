using Dev.Acadmy.Dtos.Request.Courses;
using Dev.Acadmy.Dtos.Response.Courses;
using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Courses
{
    public class CourseCertificateAppService : ApplicationService, ICourseCertificateAppService
    {
        private readonly CourseCertificateManager _certificateManager;
        private readonly ICourseCertificateRepository _courseCertificateRepository;
        private readonly IMediaItemRepository _mediaItemRepository;

        public CourseCertificateAppService(
            CourseCertificateManager certificateManager,
            ICourseCertificateRepository courseCertificateRepository,
            IMediaItemRepository mediaItemRepository)
        {
            _certificateManager = certificateManager;
             _courseCertificateRepository = courseCertificateRepository;
            _mediaItemRepository = mediaItemRepository;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task CreateOrUpdateAsync([FromForm] CreateUpdateCourseCertificateDto input)
        {
            // نمرر المهمة للمدير (Manager)
            await _certificateManager.CreateOrUpdateAsync(
                input.CourseId,
                input.TemplateFile,
                input.NameXPosition,
                input.NameYPosition
            );
        }

        // دالة التحميل للطالب
        [HttpGet]
        [Authorize]
        public async Task<IRemoteStreamContent> DownloadCertificateAsync(Guid courseId)
        {
            // 1. جلب بيانات الشهادة
            var cert = await _courseCertificateRepository.GetAsync(c => c.CourseId == courseId);

            // 2. جلب اسم الطالب الحالي من الـ Session
            var studentName = $"{CurrentUser.Name}";

            if (string.IsNullOrWhiteSpace(studentName)) studentName = "Student Name";

            var templateUrl = (await _mediaItemRepository.FirstOrDefaultAsync(x=>x.RefId == cert.Id))?.Url?? string.Empty;
            // 3. توليد ملف الـ PDF (نمرر الرابط والإحداثيات والاسم)
            var pdfBytes = await _courseCertificateRepository.GeneratePdfWithTextAsync(
                templateUrl,
                studentName,
                cert.NameXPosition,
                cert.NameYPosition
            );

            // 4. إرجاع الملف كـ Stream ليتم تحميله في المتصفح
            return new RemoteStreamContent(
                new MemoryStream(pdfBytes),
                fileName: $"Certificate_{courseId}.pdf",
                contentType: "application/pdf"
            );
        }

        [Authorize]
        public async Task<CourseCertificateDto> GetByCourseIdAsync(Guid courseId)
        {
            var cert = await _courseCertificateRepository.FirstOrDefaultAsync(x=>x.CourseId == courseId);
            var mediaItem = await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == cert.Id);
            return new CourseCertificateDto
            {
                Id = cert.Id,
                CourseId = cert.CourseId,
                NameXPosition = cert.NameXPosition,
                NameYPosition = cert.NameYPosition,
                TemplateUrl = mediaItem != null ? mediaItem.Url : string.Empty
            };
        }
    }
}
