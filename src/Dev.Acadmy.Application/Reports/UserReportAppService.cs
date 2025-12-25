using AutoMapper;
using Dev.Acadmy.Dtos.Request.Reports;
using Dev.Acadmy.Dtos.Response.Reports;
using Dev.Acadmy.Entities.Reports;
using Dev.Acadmy.Entities.Reports.Entities;
using Dev.Acadmy.Enums;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Users;

namespace Dev.Acadmy.Reports
{
    [Authorize] // حماية الخدمة بالكامل
    public class UserReportAppService : ApplicationService, IUserReportAppService
    {
        private readonly IUserReportRepository _reportRepository;
        private readonly IMapper _mapper;

        public UserReportAppService(
            IUserReportRepository reportRepository,
            IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }

        #region User Methods

        public async Task<ResponseApi<ReportDto>> CreateAsync(CreateUpdateReportDto input)
        {
            var report = new UserReport
            {
                Title = input.Title,
                Description = input.Description,
                Type =(ReportType) input.Type,
                TargetEntityId = input.TargetEntityId,
                UserId = CurrentUser.GetId(),
                Status = ReportStatus.Pending // الحالة الافتراضية
            };

            var result = await _reportRepository.InsertAsync(report, autoSave: true);
            var dto = _mapper.Map<UserReport, ReportDto>(result);

            return new ResponseApi<ReportDto>
            {
                Data = dto,
                Success = true,
                Message = "تم إرسال التقرير بنجاح"
            };
        }

        public async Task<ResponseApi<ReportDto>> UpdateAsync(Guid id, CreateUpdateReportDto input)
        {
            var report = await _reportRepository.GetAsync(id);

            // التأكد أن المستخدم هو صاحب التقرير
            if (report.UserId != CurrentUser.GetId())
            {
                throw new UserFriendlyException("غير مسموح لك بتعديل هذا التقرير");
            }

            report.Title = input.Title;
            report.Description = input.Description;
            report.Type = (ReportType)input.Type;
            report.TargetEntityId = input.TargetEntityId;

            var result = await _reportRepository.UpdateAsync(report);
            var dto = _mapper.Map<UserReport, ReportDto>(result);

            return new ResponseApi<ReportDto>
            {
                Data = dto,
                Success = true,
                Message = "تم تحديث التقرير بنجاح"
            };
        }

        public async Task<PagedResultDto<ReportDto>> GetMyReportsAsync(int pageNumber, int pageSize)
        {
            var userId = CurrentUser.GetId();

            // حساب عدد العناصر التي سيتم تخطيها
            int skipCount = (pageNumber - 1) * pageSize;

            var (items, totalCount) = await _reportRepository.GetListByUserIdAsync(
                userId,
                skipCount,
                pageSize);

            return new PagedResultDto<ReportDto>(
                totalCount,
                _mapper.Map<List<UserReport>, List<ReportDto>>(items)
            );
        }

        #endregion

        #region Admin Methods

        [Authorize(Roles = "admin")] // متاح فقط للمشرفين
        public async Task<PagedResultDto<ReportDto>> GetListAsync(int? type, int? status, int pageNumber, int pageSize)
        {
            int skipCount = (pageNumber - 1) * pageSize;

            var (items, totalCount) = await _reportRepository.GetListAsync(
                (ReportType?)type,
                (ReportStatus?)status,
                skipCount,
                pageSize);

            return new PagedResultDto<ReportDto>(
                totalCount,
                _mapper.Map<List<UserReport>, List<ReportDto>>(items)
            );
        }

        public async Task<ResponseApi<ReportDto>> GetAsync(Guid id)
        {
            var report = await _reportRepository.GetAsync(id);
            var dto = _mapper.Map<UserReport, ReportDto>(report);

            return new ResponseApi<ReportDto>
            {
                Data = dto,
                Success = true
            };
        }

        [Authorize(Roles = "admin")]
        public async Task<ResponseApi<ReportDto>> UpdateStatusAsync(Guid id, int status)
        {
            var report = await _reportRepository.GetAsync(id);
            report.Status = (ReportStatus)status;

            await _reportRepository.UpdateAsync(report);
            var dto = _mapper.Map<UserReport, ReportDto>(report);

            return new ResponseApi<ReportDto>
            {
                Data = dto,
                Success = true,
                Message = "تم تحديث حالة التقرير بنجاح"
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var report = await _reportRepository.GetAsync(id);

            // السماح بالحذف لصاحب التقرير أو الأدمن
            if (report.UserId != CurrentUser.GetId() && !CurrentUser.IsInRole("admin"))
            {
                throw new UserFriendlyException("غير مسموح لك بحذف هذا التقرير");
            }

            await _reportRepository.DeleteAsync(id);
        }

        #endregion
    }
}