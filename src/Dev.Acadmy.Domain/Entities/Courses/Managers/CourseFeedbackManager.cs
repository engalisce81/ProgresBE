using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Interfaces;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Entities.Courses.Managers
{
    public class CourseFeedbackManager : DomainService
    {
        private readonly ICourseFeedbackRepository _feedbackRepo;
        private readonly ICourseStudentRepository _courseStudentRepo;
        private readonly IMediaItemRepository _mediaItemRepo;
        public CourseFeedbackManager(
            ICourseFeedbackRepository feedbackRepo,
            ICourseStudentRepository courseStudentRepo,
            IMediaItemRepository mediaItemRepo)
        {
            _feedbackRepo = feedbackRepo;
            _courseStudentRepo = courseStudentRepo;
            _mediaItemRepo = mediaItemRepo;
        }

        /// <summary>
        /// إنشاء تقييم جديد مع التحقق من شروط الاشتراك وعدم التكرار
        /// </summary>
        public async Task<CourseFeedback> CreateAsync(Guid courseId, Guid userId, int rating, string comment)
        {
            // 1. التحقق من اشتراك الطالب وقبوله في الكورس
            var isEnrolled = await _courseStudentRepo.AnyAsync(x =>
                x.CourseId == courseId && x.UserId == userId && x.IsSubscibe);

            if (!isEnrolled)
                throw new UserFriendlyException("يجب الاشتراك في الكورس أولاً لتتمكن من التقييم");

            // 2. منع الطالب من إضافة أكثر من تقييم لنفس الكورس
            var alreadyExist = await _feedbackRepo.AnyAsync(x =>
                x.CourseId == courseId && x.UserId == userId);

            if (alreadyExist)
                throw new UserFriendlyException("لقد قمت بتقييم هذا الكورس مسبقاً");

            // إنشاء التقييم (الحالة الافتراضية غير مقبول حتى يراجعه الأدمن)
            return new CourseFeedback(courseId, userId, rating, comment)
            {
                IsAccept = false
            };
        }

        /// <summary>
        /// تحديث بيانات التقييم (صاحب التقييم أو الأدمن فقط)
        /// </summary>
        public async Task<CourseFeedback> UpdateAsync(Guid feedbackId, Guid currentUserId, bool isAdmin, int newRating, string newComment)
        {
            var feedback = await _feedbackRepo.GetAsync(feedbackId);

            // التحقق من صلاحية التعديل
            if (feedback.UserId != currentUserId && !isAdmin)
                throw new UserFriendlyException("لا تملك صلاحية تعديل هذا التقييم");

            feedback.Rating = newRating;
            feedback.Comment = newComment;

            // إذا قام الطالب بالتعديل، يعود التقييم للمراجعة (IsAccept = false)
            // أما إذا كان الأدمن هو من يعدل، تبقى الحالة كما هي أو تُحدد يدوياً
            if (!isAdmin)
            {
                feedback.IsAccept = false;
            }

            return feedback;
        }

        /// <summary>
        /// تغيير حالة قبول التقييم (للأدمن فقط)
        /// </summary>
        public async Task ChangeAcceptanceStatusAsync(Guid feedbackId, bool isAccepted, bool isAdmin)
        {
            if (!isAdmin)
                throw new UserFriendlyException("وحده الآدمن يمكنه تغيير حالة قبول التقييمات");

            var feedback = await _feedbackRepo.GetAsync(feedbackId);
            feedback.IsAccept = isAccepted;
        }

        /// <summary>
        /// التحقق من سياسة الحذف (صاحب التقييم أو الأدمن فقط)
        /// </summary>
        public async Task CheckDeletePolicyAsync(Guid feedbackId, Guid currentUserId, bool isAdmin)
        {
            var feedback = await _feedbackRepo.GetAsync(feedbackId);

            if (feedback.UserId != currentUserId && !isAdmin)
            {
                throw new UserFriendlyException("لا تملك صلاحية حذف هذا التقييم");
            }
        }

    }
}