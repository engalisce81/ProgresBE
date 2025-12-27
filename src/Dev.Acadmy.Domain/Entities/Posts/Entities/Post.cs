using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Entities.Posts.Entities
{
    public class Post :AuditedAggregateRoot<Guid>
    {

        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsGeneral { get; set; } = true;
        public Guid UserId { get; set; } // من تم التفاعل على منشوره
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Reaction> Reactions { get; set; }
    }
}
