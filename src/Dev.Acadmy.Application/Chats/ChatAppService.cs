using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Dev.Acadmy.Entities.Chats.Entites;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Mvc;
using Dev.Acadmy.Dtos.Request.Chats;

namespace Dev.Acadmy.Chats
{
    public class ChatAppService : ApplicationService
    {
        private readonly IRepository<ChatMessage, Guid> _chatRepo;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatAppService(
            IRepository<ChatMessage, Guid> chatRepo,
            IHubContext<ChatHub> hubContext)
        {
            _chatRepo = chatRepo;
            _hubContext = hubContext;
        }

        public async Task SendMessageAsync(CreateUpdateChatMessageDto input)
        {
            // التحقق من هوية المرسل
            var senderId = CurrentUser.GetId();
            var senderName = CurrentUser.Name ?? CurrentUser.UserName;

            // 1. حفظ الرسالة في قاعدة البيانات
            var chatMsg = new ChatMessage
            {
                ReceverId = input.ReceverId, // المعرف الخاص بالجروب (الكورس)
                SenderId = senderId,
                Message = input.Message,
                // يتم ملء وقت الإنشاء تلقائياً إذا كنت ترث من FullAuditedEntity
            };

            await _chatRepo.InsertAsync(chatMsg);

            // 2. إرسال الرسالة فوراً عبر SignalR للمتواجدين حالياً في الجروب
            // نستخدم input.ReceverId كاسم للمجموعة (Group Name)
            await _hubContext.Clients.Group(input.ReceverId.ToString()).SendAsync("ReceiveMessage", new
            {
                senderId = senderId,
                senderName = senderName,
                content = input.Message,
                time = DateTime.Now.ToString("HH:mm")
            });
        }
    }
}