using Dev.Acadmy.Dtos.Request.Courses;
using Dev.Acadmy.Dtos.Response.Courses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseCertificateAppService : IApplicationService
    {
        // للمعلّم: رفع قالب الشهادة وتحديد إحداثيات الاسم
        Task CreateOrUpdateAsync(CreateUpdateCourseCertificateDto input);

        // للطالب: الحصول على الشهادة بصيغة PDF مع اسمه
        Task<IRemoteStreamContent> DownloadCertificateAsync(Guid courseId);

        // للحصول على الإعدادات الحالية (إذا أراد المعلم تعديلها)
        Task<CourseCertificateDto> GetByCourseIdAsync(Guid courseId);
    }
}
