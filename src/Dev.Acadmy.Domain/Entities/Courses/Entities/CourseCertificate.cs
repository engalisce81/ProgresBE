using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Entities.Courses.Entities
{
    public class CourseCertificate :AuditedAggregateRoot<Guid>
    {
        public Guid CourseId { get; set; }
        public double NameXPosition { get; set; } // النسبة المئوية X
        public double NameYPosition { get; set; } // النسبة المئوية Y
        public float FontSize { get; set; } = 24;
        [ForeignKey(nameof(CourseId))]
        public virtual Course Course { get; set; }

        private CourseCertificate() { } // لـ ABP

        public CourseCertificate(Guid courseId, double x, double y)
        {
            CourseId = courseId;
            UpdateSettings( x, y);
        }

        public void UpdateSettings( double x, double y)
        {
            NameXPosition = x;
            NameYPosition = y;
        }
    }
}
