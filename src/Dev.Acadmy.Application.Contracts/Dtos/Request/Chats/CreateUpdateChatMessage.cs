using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Request.Chats
{
    public class CreateUpdateChatMessageDto
    {
        public Guid ReceverId { get; set; } // المعرف الخاص بالجروب (الكورس)
        public string Message { get; set; }
    }
}
