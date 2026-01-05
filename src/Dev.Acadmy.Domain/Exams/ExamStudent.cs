using System;
using System.ComponentModel.DataAnnotations.Schema;
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
        public DateTime FinishedAt { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser User { get; set; }
        [ForeignKey(nameof(ExamId))]
        public virtual Exam Exam { get; set; }
    }
}
