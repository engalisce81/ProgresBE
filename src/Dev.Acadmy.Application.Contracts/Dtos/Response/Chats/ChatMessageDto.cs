using System;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Chats
{
    public class ChatMessageDto : EntityDto<Guid>
    {
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string LogoUrl { get; set; }
        public Guid ReceverId { get; set; }
        public string Message { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
