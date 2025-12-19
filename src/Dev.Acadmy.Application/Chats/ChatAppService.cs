using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Dev.Acadmy.Entities.Chats.Entites;
using Volo.Abp.Users;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.EventBus.Local;
using Dev.Acadmy.Dtos.Request.Chats;
using Volo.Abp.Application.Dtos;
using Dev.Acadmy.Dtos.Response.Chats;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Dev.Acadmy.Chats
{
    public class ChatAppService : ApplicationService
    {
        private readonly IRepository<ChatMessage, Guid> _chatRepo;
        private readonly ILocalEventBus _localEventBus; // الحل الثاني: الحقن الصريح

        public ChatAppService(
            IRepository<ChatMessage, Guid> chatRepo,
            ILocalEventBus localEventBus) // حقن الواجهة في الـ Constructor
        {
            _chatRepo = chatRepo;
            _localEventBus = localEventBus;
        }

        [Authorize]
        public async Task SendMessageAsync(CreateUpdateChatMessageDto input)
        {
            var senderId = CurrentUser.GetId();
            var senderName = CurrentUser.Name ?? CurrentUser.UserName;

            // 1. حفظ الرسالة في قاعدة البيانات
            var chatMsg = new ChatMessage
            {
                ReceverId = input.ReceverId,
                SenderId = senderId,
                Message = input.Message
            };
            await _chatRepo.InsertAsync(chatMsg);

            // 2. نشر الحدث باستخدام الـ _localEventBus المحقون
            await _localEventBus.PublishAsync(new NewChatMessageEto
            {
                SenderId = senderId,
                SenderName = senderName,
                ReceverId = input.ReceverId,
                Message = input.Message
            });
        }

        /// <summary>
        /// جلب رسائل محادثة معينة (جروب أو كورس) مع Pagination
        /// </summary>
        [Authorize]
        public async Task<PagedResultDto<ChatMessageDto>> GetMessagesAsync(
     Guid receverId,
     int pageNumber = 1,
     int pageSize = 10,
     string search = null)
        {
            // 1. حساب الـ Skip بناءً على رقم الصفحة
            // إذا كانت الصفحة 1 والـ size 10 -> skip = 0
            var skipCount = (pageNumber - 1) * pageSize;

            // 2. إنشاء الاستعلام الأساسي
            var query = await _chatRepo.GetQueryableAsync();

            // 3. الفلترة حسب الـ ReceverId
            query = query.Where(x => x.ReceverId == receverId);

            // 4. إضافة ميزة البحث إذا كانت القيمة غير فارغة
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Message.Contains(search));
            }

            // 5. حساب العدد الإجمالي للنتائج المفلترة
            var totalCount = await query.CountAsync();

            // 6. جلب البيانات المرتبة والمقسمة لصفحات
            var messages = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();

            // 7. تحويل النتائج إلى DTO
            var dtos = messages.Select(x => new ChatMessageDto
            {
                Id = x.Id,
                SenderId = x.SenderId,
                Message = x.Message,
                CreationTime = x.CreationTime,
                ReceverId = x.ReceverId
            }).ToList();

            return new PagedResultDto<ChatMessageDto>(totalCount, dtos);
        }
    }
}
public class NewChatMessageEto
{
    public Guid SenderId { get; set; }
    public string SenderName { get; set; }
    public Guid ReceverId { get; set; }
    public string Message { get; set; }
}