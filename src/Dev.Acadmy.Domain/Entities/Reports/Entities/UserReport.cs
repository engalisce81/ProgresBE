using Dev.Acadmy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Entities.Reports.Entities
{
    public class UserReport : FullAuditedEntity<Guid>
    {
        public string Title { get; set; }        // عنوان التقرير
        public string Description { get; set; }  // وصف المشكلة أو الاقتراح
        public ReportType Type { get; set; }      // النوع (شكوى، اقتراح، بلاغ)
        public ReportStatus Status { get; set; }  // حالة التقرير
        // ربط التقرير بالمستخدم الذي أرسله
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
        // اختياري: إذا كان البلاغ متعلق بكيان آخر (مثل منشور معين)
        public Guid? TargetEntityId { get; set; }
    }
}
