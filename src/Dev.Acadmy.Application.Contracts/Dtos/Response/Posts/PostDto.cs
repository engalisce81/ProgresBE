using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Posts
{
    public class PostDto : AuditedEntityDto<Guid>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsGeneral { get; set; }
        public string AuthorName { get; set; }
        public int CommentsCount { get; set; }
        public int ReactionsCount { get; set; }
        public string LogoUrl { get; set; }
        public int TotalCommentsCount { get; set; }
        public List<ReactionSummaryDto> ReactionsSummaries { get; set; }
    }
   
}
