using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Universites
{
    public  class Subject : AuditedAggregateRoot<Guid>  
    {
        public string Name { get; set; }
        public Guid? TermId { get; set;}
        public Guid? GradeLevelId { get; set;}
        [ForeignKey(nameof(GradeLevelId))]
        public GradeLevel? GradeLevel{ get; set;}
        [ForeignKey(nameof(TermId))]
        public Term? Term { get; set;}
        public ICollection<Entities.Courses.Entities.Course> Courses { get; set; } = new List<Entities.Courses.Entities.Course>();
    }
}
