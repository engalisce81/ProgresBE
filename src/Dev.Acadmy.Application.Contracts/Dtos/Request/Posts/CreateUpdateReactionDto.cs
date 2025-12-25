using System;

namespace Dev.Acadmy.Dtos.Request.Posts
{
    public class CreateUpdateReactionDto
    {
        public Guid PostId { get; set; }
        public int Type { get; set; } // Enum: Like=1, Love=2...    
    }
}
