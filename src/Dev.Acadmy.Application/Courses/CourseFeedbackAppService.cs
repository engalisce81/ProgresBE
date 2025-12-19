using Dev.Acadmy.Dtos.Request.Courses;
using Dev.Acadmy.Dtos.Response.Courses;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Permissions; // تأكد من استدعاء ملف الـ Permissions
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace Dev.Acadmy.Services.Courses
{
    namespace Dev.Acadmy.Services.Courses
    {
        [Authorize(AcadmyPermissions.CourseFeedbacks.Default)]
        public class CourseFeedbackAppService : ApplicationService, ICourseFeedbackAppService
        {
            private readonly ICourseFeedbackRepository _feedbackRepo;
            private readonly CourseFeedbackManager _feedbackManager;
            private readonly IMediaItemRepository _mediaItemRepo;

            public CourseFeedbackAppService(
                ICourseFeedbackRepository feedbackRepo,
                CourseFeedbackManager feedbackManager,
                IMediaItemRepository mediaItemRepo)
            {
                _feedbackRepo = feedbackRepo;
                _feedbackManager = feedbackManager;
                _mediaItemRepo = mediaItemRepo;
            }

            [Authorize(AcadmyPermissions.CourseFeedbacks.Create)]
            public async Task<ResponseApi<bool>> CreateAsync(CreateUpdateCourseFeedbackDto input)
            {
                var feedback = await _feedbackManager.CreateAsync(
                    input.CourseId,
                    CurrentUser.GetId(),
                    input.Rating,
                    input.Comment
                );

                await _feedbackRepo.InsertAsync(feedback);

                return new ResponseApi<bool>
                {
                    Data = true,
                    Success = true,
                    Message = L["Feedback:CreatedSuccessfully"]
                };
            }


            [Authorize(AcadmyPermissions.CourseFeedbacks.View)]
            public async Task<PagedResultDto<FeedbackDto>> GetFeedbacksByCourseId(Guid courseId, int pageNumber, int pageSize, bool isAccept)
            {
                // 1. جلب التقييمات المرتبطة بالكورس
                var feedbacks = await _feedbackRepo.GetListFeedbacksByCourseIdAsync(courseId,pageNumber,pageSize , isAccept);
                // 3. جلب صور المستخدمين بناءً على الـ UserIds
                var userIds = feedbacks.Select(x => x.UserId).ToList();
                var mediaDic = await _mediaItemRepo.GetUrlDictionaryByRefIdsAsync(userIds);
                // 4. ربط الصور بالـ DTOs داخل اللوب
                foreach (var dto in feedbacks)
                {
                    // نتحقق إذا كان المستخدم له صورة في القاموس، إذا لم يوجد نضع صورة افتراضية
                    if (mediaDic.TryGetValue(dto.UserId, out var imageUrl))    dto.LogoUrl = imageUrl; 
                }
                // 5. إرجاع النتيجة بتنسيق PagedResultDto
                return new PagedResultDto<FeedbackDto>(
                    feedbacks.Count, // إجمالي العدد
                    feedbacks        // القائمة
                );
            }

            [Authorize(AcadmyPermissions.CourseFeedbacks.Edit)]
            public async Task<ResponseApi<bool>> UpdateAsync(Guid id, CreateUpdateCourseFeedbackDto input)
            {
                var feeedback = await _feedbackManager.UpdateAsync(
                    id,
                    CurrentUser.GetId(),
                    CurrentUser.IsInRole(RoleConsts.Admin.ToLower()),
                    input.Rating,
                    input.Comment
                );
                await _feedbackRepo.UpdateAsync(feeedback, autoSave: true);
                return new ResponseApi<bool>
                {
                    Data = true,
                    Success = true,
                    Message = L["Feedback:UpdatedSuccessfully"]
                };
            }

            [Authorize(AcadmyPermissions.CourseFeedbacks.Delete)]
            public async Task<ResponseApi<bool>> DeleteAsync(Guid id)
            {
                await _feedbackManager.CheckDeletePolicyAsync(
                    id,
                    CurrentUser.GetId(),
                    CurrentUser.IsInRole(RoleConsts.Admin)
                );

                await _feedbackRepo.DeleteAsync(id);

                return new ResponseApi<bool>
                {
                    Data = true,
                    Success = true,
                    Message = L["Feedback:DeletedSuccessfully"]
                };
            }

            [Authorize(AcadmyPermissions.CourseFeedbacks.Accept)]
            public async Task<ResponseApi<bool>> AcceptFeedbackAsync(Guid id, bool isAccept)
            {
                await _feedbackManager.ChangeAcceptanceStatusAsync(
                    id,
                    isAccept,
                    CurrentUser.IsInRole(RoleConsts.Admin.ToLower())
                );

                return new ResponseApi<bool>
                {
                    Data = true,
                    Success = true,
                    Message = L["Feedback:StatusChanged"]
                };
            }
        }
    }
}
