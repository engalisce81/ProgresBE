using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Questions
{
    public class QuestionBank :AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public Guid? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public IdentityUser? User { get;set; }
        public ICollection<Question> Questions { get; set; }=new List<Question>();
    }
}
