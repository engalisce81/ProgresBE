using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Dev.Acadmy.Entities.Chats.Entites; // تأكد من صحة هذا المسار الـ Namespace للـ Entity

namespace Dev.Acadmy.Chats
{
    [HubRoute("/chat-hub")]
    [Authorize]
    public class ChatHub : AbpHub
    {
        private readonly IRepository<ChatMessage, Guid> _chatRepo;

        public ChatHub(IRepository<ChatMessage, Guid> chatRepo)
        {
            _chatRepo = chatRepo;
        }

        // 1. الانضمام لجروب (بناءً على الكورس)
        public async Task JoinCourseGroup(Guid courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, courseId.ToString());
        }

        // 2. إرسال رسالة وحفظها
        public async Task SendMessage(Guid courseId, string message)
        {
            var senderId = CurrentUser.GetId();

            // حفظ في الداتابيز ليراها المستخدم لاحقاً
            var chatMsg = new ChatMessage
            {
                ReceverId = courseId, // لاحظ التأكد من كتابة الاسم صح في الـ Entity (ReceiverId)
                SenderId = senderId,
                Message = message
            };

            // ABP repositories تتعامل مع الـ Unit of Work تلقائياً هنا
            await _chatRepo.InsertAsync(chatMsg);

            // إرسال فورياً لكل أعضاء الجروب
            // تم تصحيح استدعاء Clients ليكون التابع لـ SignalR مباشرة
            await Clients.Group(courseId.ToString()).SendAsync("ReceiveMessage", new
            {
                senderId = senderId,
                content = message,
                time = DateTime.Now.ToString("HH:mm")
            });
        }
    }
}