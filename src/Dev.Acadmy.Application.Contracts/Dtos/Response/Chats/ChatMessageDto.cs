using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Chats
{
    public class ChatMessageDto : EntityDto<Guid>
    {
        public Guid SenderId { get; set; }
        public Guid ReceverId { get; set; }
        public string Message { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
