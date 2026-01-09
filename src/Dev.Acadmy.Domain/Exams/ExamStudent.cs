using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Exams
{
    public class ExamStudent : FullAuditedEntity<Guid>
    {
        public Guid ExamId { get; set; }
        public Guid UserId { get; set; }
        public double Score { get; set; }
        public int TryCount { get; set; }
        public bool IsPassed { get; set; }
        public DateTime FinishedAt { get; set; }= DateTime.Now;

        // الحقول الجديدة
        public bool IsCertificateIssued { get; private set; }
        public DateTime? LastCertificateRequestDate { get; private set; }

        [ForeignKey(nameof(ExamId))]
        public Exam Exam { get; set; }
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        // Domain Method: للتحقق من إمكانية طلب الشهادة
        public bool CanRequestCertificate()
        {
            if (!IsPassed) return false;
            if (!IsCertificateIssued) return true;

            // شرط الـ 24 ساعة
            return !LastCertificateRequestDate.HasValue ||
                   (DateTime.Now - LastCertificateRequestDate.Value).TotalDays >= 1;
        }

        // Domain Method: لتنفيذ عملية الطلب (تغيير الحالة)
        public void IssueCertificate()
        {
            if (!CanRequestCertificate())
            {
                throw new BusinessException("Exam:Wait24Hours")
                    .WithData("NextDate", LastCertificateRequestDate?.AddDays(1));
            }

            IsCertificateIssued = true;
            LastCertificateRequestDate = DateTime.Now;
        }
    }
}
