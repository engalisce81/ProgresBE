using Dev.Acadmy.Dtos.Request.Courses;
using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Permissions; // تأكد من استدعاء ملف الـ Permissions
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
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

            public CourseFeedbackAppService(
                ICourseFeedbackRepository feedbackRepo,
                CourseFeedbackManager feedbackManager)
            {
                _feedbackRepo = feedbackRepo;
                _feedbackManager = feedbackManager;
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

            [Authorize(AcadmyPermissions.CourseFeedbacks.Edit)]
            public async Task<ResponseApi<bool>> UpdateAsync(Guid id, CreateUpdateCourseFeedbackDto input)
            {
                var feeedback = await _feedbackManager.UpdateAsync(
                    id,
                    CurrentUser.GetId(),
                    CurrentUser.IsInRole(RoleConsts.Admin),
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
