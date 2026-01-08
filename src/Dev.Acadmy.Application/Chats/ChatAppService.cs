using Dev.Acadmy.Dtos.Request.Chats;
using Dev.Acadmy.Dtos.Response.Chats;
using Dev.Acadmy.Entities.Chats.Entites;
using Dev.Acadmy.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.Chats
{
    public class ChatAppService : ApplicationService
    {
        private readonly IRepository<ChatMessage, Guid> _chatRepo;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly IdentityUserManager _userManager; // تأكد من الاسم الصحيح
        private readonly IRepository<IdentityUser, Guid> _userRepository;

        public ChatAppService(
            IRepository<ChatMessage, Guid> chatRepo,
            IMediaItemRepository mediaItemRepository,
            IdentityUserManager userManager,
            IRepository<IdentityUser, Guid> userRepository)
        {
            _chatRepo = chatRepo;
            _mediaItemRepository = mediaItemRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        [Authorize]
        public async Task<ChatMessageDto> SendMessageAsync(CreateUpdateChatMessageDto input)
        {
            var senderId = CurrentUser.GetId();

            // 1. تحديد هل المرسل Instructor (من الـ Token مباشرة للأداء)
            var isSenderInstructor = CurrentUser.IsInRole(RoleConsts.Teacher);

            // 3. حفظ الرسالة في قاعدة البيانات
            var chatMsg = new ChatMessage
            {
                ReceverId = input.ReceverId,
                SenderId = senderId,
                Message = input.Message,
                IsSenderInstructor = isSenderInstructor,
            };
            await _chatRepo.InsertAsync(chatMsg);

            // 4. جلب صورة المرسل
            var mediaItemSender = await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == senderId);

            // 5. تجهيز الـ DTO
            var messageDto = new ChatMessageDto
            {
                Id = chatMsg.Id,
                SenderId = senderId,
                SenderName = CurrentUser?.Name ?? CurrentUser?.UserName??string.Empty,
                ReceverId = input.ReceverId,
                Message = input.Message,
                CreationTime = DateTime.Now,
                LogoUrl = mediaItemSender?.Url ?? string.Empty,
                IsInstructor = isSenderInstructor // مبرمج الفلاتر سيعتمد على هذه لتغيير شكل الفقاعة
            };

            // 6. النشر عبر الـ Local Event Bus (مفيد للـ Real-time أو الـ Notifications)
           // await _localEventBus.PublishAsync(messageDto);

            return messageDto;
        }

        /// <summary>
        /// جلب رسائل محادثة معينة (جروب أو كورس) مع Pagination
        /// </summary>
        [Authorize]
        public async Task<ChatMessageDto> UpdateMessageAsync(Guid id, CreateUpdateChatMessageDto input)
        {
            // 1. جلب الرسالة من قاعدة البيانات
            var chatMsg = await _chatRepo.GetAsync(id);

            // 3. تحديث نص الرسالة
            chatMsg.Message = input.Message;

            // يمكنك إضافة حقل IsEdited في جدول ChatMessage إذا أردت إظهار كلمة "معدلة"
            // chatMsg.IsEdited = true;

            await _chatRepo.UpdateAsync(chatMsg);

            // 4. إرسال تحديث عبر SignalR (اختياري ولكن مهم لمبرمج الفلاتر)
            // حتى يرى الطرف الآخر التعديل فوراً
            var messageDto = new ChatMessageDto
            {
                Id = chatMsg.Id,
                SenderId = chatMsg.SenderId,
                ReceverId = chatMsg.ReceverId,
                Message = chatMsg.Message,
                CreationTime = chatMsg.CreationTime,
                // يمكنك جلب الصورة والاسم هنا أيضاً إذا لزم الأمر
            };
            return messageDto;

        }
        [Authorize]
        public async Task<PagedResultDto<ChatMessageDto>> GetMessagesAsync(
            Guid receverId,
            int pageNumber = 1,
            int pageSize = 10,
            string search = null)
        {
            var skipCount = (pageNumber - 1) * pageSize;
            var query = await _chatRepo.GetQueryableAsync();

            // 1. فلترة الرسائل الخاصة بالمستلم
            query = query.Where(x => x.ReceverId == receverId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Message.Contains(search));
            }

            var totalCount = await query.CountAsync();

            // 2. جلب الرسائل (مرتبة من الأحدث إلى الأقدم عادة في الشات)
            var messages = await query
                .OrderByDescending(x => x.CreationTime) // ترتيب تنازلي لرؤية أحدث الرسائل
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();

            // 3. جلب معرفات المرسلين الفريدة
            var senderIds = messages.Select(x => x.SenderId).Distinct().ToList();

            // 4. جلب الصور دفعة واحدة (القاموس الحالي)
            var mediItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(senderIds);

            // 5. الحل: جلب أسماء المرسلين دفعة واحدة من جدول المستخدمين
            // نفترض أنك تستخدم IRepository<IdentityUser, Guid> _userRepository
            var usersDic = await (await _userRepository.GetQueryableAsync())
                .Where(u => senderIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Name }) // جلب الحقول المطلوبة فقط
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            // 6. تحويل النتائج ودمج الاسم والصورة
            var dtos = messages.Select(x => new ChatMessageDto
            {
                Id = x.Id,
                SenderId = x.SenderId,
                // نبحث في قاموس المستخدمين عن الاسم
                SenderName = usersDic.GetValueOrDefault(x.SenderId) ?? "Unknown User",
                Message = x.Message,
                CreationTime = x.CreationTime,
                ReceverId = x.ReceverId,
                IsInstructor = x.IsSenderInstructor,
                // نبحث في قاموس الصور عن الرابط
                LogoUrl = mediItemDic.GetValueOrDefault(x.SenderId) ?? string.Empty
            }).ToList();

            return new PagedResultDto<ChatMessageDto>(totalCount, dtos);
        }

        [Authorize]
        public async Task DeleteMessageAsync(Guid id)
        {
            // 1. جلب الرسالة من قاعدة البيانات
            var chatMsg = await _chatRepo.GetAsync(id);

           
            // 3. تنفيذ الحذف
            // إذا كنت تستخدم Soft Delete (حذف منطقي) ستختفي من الاستعلامات تلقائياً
            await _chatRepo.DeleteAsync(chatMsg);

            // 4. إشعار مبرمج الفلاتر (SignalR)
            // نرسل الـ Id الخاص بالرسالة المحذوفة ليقوم بحذفها من القائمة في الموبايل
            /*
            await _chatHubContext.Clients.User(chatMsg.ReceverId.ToString())
                                 .SendAsync("MessageDeleted", new { id = chatMsg.Id });
            */
        }

    }
}
