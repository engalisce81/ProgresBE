using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Entities.Chats.Entites
{
    public class ChatMessage : AuditedEntity<Guid>
    {
        public Guid ReceverId { get; set; } // المعرف الخاص بالجروب (الكورس) أو المستخدم
        public string Message { get; set; }
        public Guid SenderId { get; set; }

        // الحقول الجديدة
        public bool IsSenderInstructor { get; set; }   // هل المرسل مدرب؟
    }
}
