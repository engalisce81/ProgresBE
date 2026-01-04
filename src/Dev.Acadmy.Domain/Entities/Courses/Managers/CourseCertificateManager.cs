using Dev.Acadmy.Entities.Courses.Entities;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Dev.Acadmy.Interfaces;

namespace Dev.Acadmy.Entities.Courses.Managers
{
    public class CourseCertificateManager : DomainService
    {
        private readonly IRepository<CourseCertificate, Guid> _courseCertificateRepository;
        private readonly IMediaItemRepository _mediaItemRepository;
        public CourseCertificateManager(IRepository<CourseCertificate, Guid> courseCertificateRepository, IMediaItemRepository mediaItemRepository)
        {
            _courseCertificateRepository = courseCertificateRepository;
            _mediaItemRepository = mediaItemRepository;
        }

        public async Task<CourseCertificate> CreateOrUpdateAsync(
            Guid courseId,
            IFormFile file,
            double x,
            double y)
        {


            // 2. منطق البحث والتحديث
            var cert = await _courseCertificateRepository.FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (cert == null)
            {
                var result = await _courseCertificateRepository.InsertAsync(
                    new CourseCertificate(courseId, x, y)
                );
                var fileName = await _mediaItemRepository.InsertAsync(file, result.Id);
                return result;

            }
            else
            {
                cert.UpdateSettings(x, y);
                var result = await _courseCertificateRepository.UpdateAsync(cert);
                var fileName = await _mediaItemRepository.UpdateAsync(file, result.Id);
                return result;

            }
        }

        
    }
}
