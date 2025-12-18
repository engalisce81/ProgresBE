using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Courses
{
    public class CourseInfo :AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public Guid CourseId { get; set; }
        [ForeignKey(nameof(CourseId))]
        public Entities.Courses.Entities.Course Course { get; set; }
    }
}
