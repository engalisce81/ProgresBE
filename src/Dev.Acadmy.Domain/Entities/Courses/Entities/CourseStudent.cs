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
    public class CourseStudent :AuditedAggregateRoot<Guid>
    {
        public Guid UserId{ get; set; }
        public Guid CourseId{ get; set; }
        [ForeignKey(nameof(UserId))]    
        public IdentityUser User { get; set; }
        [ForeignKey(nameof(CourseId))] 
        public Course Course { get; set; }
        public bool IsSubscibe { get; set; }
    }
}
