using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.AspNetCore.SignalR;

namespace Dev.Acadmy.Chats
{
    [HubRoute("/chat-hub")]
    [Authorize]
    public class ChatHub : AbpHub
    {
        // مبرمج الفلاتر ينادي هذه الميثود بمجرد فتح شاشة الشات
        public async Task JoinCourseGroup(Guid courseId)
        {
            // إضافة المستخدم لغرفة خاصة بهذا الكورس
            await Groups.AddToGroupAsync(Context.ConnectionId, courseId.ToString());
        }

        // مبرمج الفلاتر ينادي هذه الميثود عند مغادرة الشاشة (اختياري)
        public async Task LeaveCourseGroup(Guid courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, courseId.ToString());
        }

        /* ملاحظة: مسحنا ميثود SendMessage من هنا 
           لأن الإرسال سيتم عبر الـ HTTP API (ChatAppService) 
           لضمان الحفظ في القاعدة وتشغيل الـ EventBus بشكل مركزي
        */
    }
}