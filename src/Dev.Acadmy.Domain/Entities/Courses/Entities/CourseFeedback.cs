using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Entities.Courses.Entities
{
    public class CourseFeedback : AuditedEntity<Guid>
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; } // من 1 إلى 5
        public string Comment { get; set; }
        public bool IsAccept { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set;}
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        protected CourseFeedback() { } // للمكتبات البرمجية

        public CourseFeedback( Guid courseId, Guid userId, int rating, string comment)
        {
            CourseId = courseId;
            UserId = userId;
            Rating = rating;
            Comment = comment;
        }
    }
}
