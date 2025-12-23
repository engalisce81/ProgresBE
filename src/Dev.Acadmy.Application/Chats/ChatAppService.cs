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
using Dev.Acadmy.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Dev.Acadmy.Chats
{
    public class ChatAppService : ApplicationService
    {
        private readonly IRepository<ChatMessage, Guid> _chatRepo;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly ILocalEventBus _localEventBus;
        //private readonly IHubContext<ChatHub> _hubContext;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatAppService(
            IRepository<ChatMessage, Guid> chatRepo,
            IMediaItemRepository mediaItemRepository,
            ILocalEventBus localEventBus)
        {
            _chatRepo = chatRepo;
            _mediaItemRepository = mediaItemRepository;
            _localEventBus = localEventBus;
            //_hubContext = hubContext;
            //_httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        public async Task SendMessageAsync(CreateUpdateChatMessageDto input)
        {
            var senderId = CurrentUser.GetId();
            var senderName = CurrentUser.Name ?? CurrentUser.UserName;

            // 1. حفظ في قاعدة البيانات
            var chatMsg = new ChatMessage
            {
                ReceverId = input.ReceverId,
                SenderId = senderId,
                Message = input.Message,
            };
            await _chatRepo.InsertAsync(chatMsg);

            // 2. جلب صورة المرسل
            var mediaItemSender = await _mediaItemRepository.FirstOrDefaultAsync(x => x.RefId == senderId);

            // 3. تجهيز الـ DTO ووضعه في List (طلب مبرمج الفلاتر)
            var messageDto = new ChatMessageDto
            {
                Id = chatMsg.Id,
                SenderId = senderId,
                SenderName = senderName,
                ReceverId = input.ReceverId,
                Message = input.Message,
                CreationTime = chatMsg.CreationTime,
                LogoUrl = mediaItemSender?.Url ?? string.Empty
            };

            var messageList = new List<ChatMessageDto> { messageDto };

            //// 4. الحل الحقيقي للـ Real-Time: إرسال عبر SignalR
            //// نرسلها لغرفة الـ Recever (سواء كان شخص أو كورس)
            //await _hubContext.Clients.Group(input.ReceverId.ToString())
            //    .SendAsync("ReceiveMessage", messageList);

            //// 5. (اختياري) استمر في استخدام الـ EventBus إذا كان لديك Handlers أخرى في السيرفر
            //await _localEventBus.PublishAsync(messageDto);
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
            var skipCount = (pageNumber - 1) * pageSize;
            var query = await _chatRepo.GetQueryableAsync();

            // فلترة الرسائل الخاصة بالمستلم (سواء كان كورس أو مستخدم)
            query = query.Where(x => x.ReceverId == receverId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Message.Contains(search));
            }

            var totalCount = await query.CountAsync();

            // جلب الرسائل
            var messages = await query
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();

            // جلب معرفات المرسلين للحصول على صورهم دفعة واحدة (Batch Request)
            var senderIds = messages.Select(x => x.SenderId).Distinct().ToList();

            // استخدام القاموس (Dictionary) الذي يربط الـ RefId بالـ Url
            var mediItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(senderIds);

            // تحويل النتائج إلى DTO ودمج الصورة من القاموس
            var dtos = messages.Select(x => new ChatMessageDto
            {
                Id = x.Id,
                SenderId = x.SenderId,
                Message = x.Message,
                CreationTime = x.CreationTime,
                ReceverId = x.ReceverId,
                // هنا الحل: نبحث في القاموس عن الـ Url باستخدام الـ SenderId
                LogoUrl = mediItemDic.ContainsKey(x.SenderId) ? mediItemDic[x.SenderId] : string.Empty
            }).ToList();

            return new PagedResultDto<ChatMessageDto>(totalCount, dtos);
        }
    }
}
