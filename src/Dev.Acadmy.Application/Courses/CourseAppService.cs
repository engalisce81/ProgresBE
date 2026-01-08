using AutoMapper;
using Dev.Acadmy.Dtos.Response.YoutubeQualities;
using Dev.Acadmy.Entities.Courses.Managers;
using Dev.Acadmy.Entities.YoutubeQualities.Managers;
using Dev.Acadmy.Enums;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Permissions;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace Dev.Acadmy.Courses
{
    public class CourseAppService :ApplicationService
    {
        private readonly CourseManager _courseManager;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseInfoRepository _courseInfoRepository;
        private readonly ICourseStudentRepository _courseStudentRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IIdentityUserRepository _userRepository;
        private readonly UnitOfWorkManager _unitOfWorkManager;
        private readonly YoutubeManager _youtubeManager; 

        private readonly IMediaItemRepository _mediaItemRepository;

        public CourseAppService(
            CourseManager courseManager,
            MediaItemManager mediaItemManager,
            IMapper mapper,
            ICurrentUser currentUser,
            IdentityUserManager userManager,
            ICourseInfoRepository courseInfoRepository,  
            UnitOfWorkManager unitOfWorkManager,
            ICourseRepository courseRepository,
            IMediaItemRepository mediaItemRepository,
            ICourseStudentRepository courseStudentRepository,
            IIdentityUserRepository userRepository,
            YoutubeManager youtubeManager)
        {
            _youtubeManager =  youtubeManager;
            _courseManager = courseManager;
            _mediaItemManager = mediaItemManager;
            _mapper = mapper;
            _currentUser = currentUser;
            _userManager = userManager;
            _courseInfoRepository = courseInfoRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _courseRepository = courseRepository;
            _mediaItemRepository = mediaItemRepository;
            _courseStudentRepository = courseStudentRepository;
            _userRepository = userRepository;
        }

        [Authorize(AcadmyPermissions.Courses.View)]
        public async Task<ResponseApi<CourseDto>> GetAsync(Guid id) => await _courseManager.GetAsync(id);

        [Authorize(AcadmyPermissions.Courses.View)]
        public async Task<PagedResultDto<CourseDto>> GetListAsync(int pageNumber, int pageSize, string? search, CourseType type)
        {
            var roles = await _userRepository.GetRoleNamesAsync(_currentUser.GetId());
            var (items, totalCount) = await _courseRepository.GetListWithDetailsAsync((pageNumber - 1) * pageSize, pageSize, search, type, _currentUser.GetId(), roles.Any(r => r.ToUpper() == RoleConsts.Admin.ToUpper()));
            var courseDtos = _mapper.Map<List<CourseDto>>(items);
            var courseIds = courseDtos.Select(c => c.Id).ToList();

            // 1. جلب بيانات الميديا والمشتركين
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(courseIds);
            var subscriberCountsDic = await _courseStudentRepository.GetTotalSubscribersPerCourseAsync(courseIds);

            foreach (var d in courseDtos)
            {
                d.LogoUrl = mediaItemDic.GetValueOrDefault(d.Id) ?? "";
                d.SubscriberCount = subscriberCountsDic.GetValueOrDefault(d.Id, 0);
            }

            return new PagedResultDto<CourseDto>(totalCount, courseDtos);
        }

        [Authorize(AcadmyPermissions.Courses.Create)]
        public async Task<ResponseApi<CourseDto>> CreateAsync(CreateUpdateCourseDto input)
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var user = await _userManager.GetByIdAsync(_currentUser.GetId());
                var course = _mapper.Map<Entities.Courses.Entities.Course>(input);
                course.UserId = user.Id; course.CollegeId = user.GetProperty<Guid>(SetPropConsts.CollegeId);

                var result = await _courseManager.CreateWithDependenciesAsync(course);

                if (input.Infos != null) foreach (var info in input.Infos) await _courseInfoRepository.InsertAsync(new CourseInfo { Name = info, CourseId = result.Id });
                if (!input.LogoUrl.IsNullOrWhiteSpace()) await _mediaItemManager.CreateAsync(new CreateUpdateMediaItemDto { Url = input.LogoUrl, RefId = result.Id, IsImage = true });

                await uow.SaveChangesAsync(); // الحفظ الفعلي في قاعدة البيانات هنا فقط
                await uow.CompleteAsync(); // إنهاء المعاملة بنجاح

                return new ResponseApi<CourseDto> { Data = _mapper.Map<CourseDto>(result), Success = true, Message = "تم الحفظ بنجاح" };
            }
        }


        [Authorize(AcadmyPermissions.Courses.Edit)]
        public async Task<ResponseApi<CourseDto>> UpdateAsync(Guid id, CreateUpdateCourseDto input)
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var courseDB = await _courseRepository.GetAsync(id);
            if (courseDB == null) return new ResponseApi<CourseDto> { Success = false, Message = "Course Not Found" };
            _mapper.Map(input, courseDB);
            courseDB.CollegeId = (await _userManager.GetByIdAsync(_currentUser.GetId())).GetProperty<Guid>(SetPropConsts.CollegeId);
            var result = await _courseManager.UpdateWithDependenciesAsync(courseDB);
            await _courseInfoRepository.DeleteByCourseIdAsync(id);
            if (input.Infos != null) foreach (var info in input.Infos) await _courseInfoRepository.InsertAsync(new CourseInfo { Name = info, CourseId = id });
            await _mediaItemManager.UpdateAsync(id, new CreateUpdateMediaItemDto { Url = input.LogoUrl, RefId = id, IsImage = true });
            await uow.SaveChangesAsync(); await uow.CompleteAsync();
            return new ResponseApi<CourseDto> { Data = _mapper.Map<CourseDto>(result), Success = true, Message = "Update Success" };
        }


        [Authorize(AcadmyPermissions.Courses.Delete)]
        public async Task DeleteAsync(Guid id) => await _courseManager.DeleteAsync(id);
        
        
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetCoursesListAsync() => await _courseManager.GetCoursesListAsync();
        [Authorize]
        public async Task<PagedResultDto<CourseInfoHomeDto>> GetCoursesInfoListAsync(int pageNumber, int pageSize, string? search ,bool alreadyJoin,Guid collegeId, Guid? subjectId, Guid? gradelevelId, Guid? termId) => await _courseManager.GetCoursesInfoListAsync(pageNumber, pageSize, search,alreadyJoin,collegeId, subjectId,  gradelevelId,termId);
        [Authorize]
        public async Task<ResponseApi<CourseInfoHomeDto>> GetCoursesInfoAsync(Guid courseId) => await _courseManager.GetCoursesInfoAsync(courseId);
        [Authorize]
        public async Task<PagedResultDto<LookupDto>> GetMyCoursesLookUpAsync() => await _courseManager.GetMyCoursesLookUpAsync();
        [Authorize]
        public async Task<Guid> DuplicateCourseAsync(Guid courseId) => await _courseManager.DuplicateCourseAsync(courseId);
        [Authorize]
        public async Task<PagedResultDto<LectureWithQuizzesDto>> GetStudentQuizzesByCourseAsync(Guid courseId, Guid userId, int pageNumber, int pageSize)=> await _courseManager.GetStudentQuizzesByCourseAsync(courseId, userId, pageNumber, pageSize);
    }
}
