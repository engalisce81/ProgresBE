using Dev.Acadmy.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Entities.Posts.Entities
{
    public class Reaction : AuditedEntity<Guid>
    {
        public ReactionType Type { get; set; } // Enum: Like=1, Love=2...
        public Guid UserId { get;set; } // من تم التفاعل على منشوره
        public Guid PostId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; }
    }
}
