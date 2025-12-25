using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Request.Posts
{
    public class CreateUpdateCommentDto
    {
        public string Text { get; set; }
        public Guid PostId { get; set; }
    }
}
