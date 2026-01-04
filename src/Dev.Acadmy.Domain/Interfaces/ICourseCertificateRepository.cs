using Dev.Acadmy.Entities.Courses.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseCertificateRepository : IRepository<CourseCertificate, Guid>
    {
        // توليد ملف PDF مع نص مخصص في إحداثيات محددة
        Task<byte[]> GeneratePdfWithTextAsync(string templateUrl, string text, double xPercent, double yPercent);
    }
}
