using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Posts
{
    public class CommentDto:AuditedEntityDto<Guid>
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public string LogoUrl { get; set; }

    }
}
