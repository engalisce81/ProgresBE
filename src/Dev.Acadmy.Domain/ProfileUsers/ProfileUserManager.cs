using Dev.Acadmy.AccountTypes;
using Dev.Acadmy.Chapters;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Lectures;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Response;
using Dev.Acadmy.Universites;
using System;

using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Dev.Acadmy.ProfileUsers
{
    public class ProfileUserManager :DomainService
    {
        private readonly ICurrentUser _currentUser;
        private readonly IIdentityUserRepository _userRepository;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IRepository<AccountType, Guid> _accountTypeRepository;
        private readonly IRepository<College, Guid> _collegeRepository;
        private readonly IRepository<University, Guid> _universityRepository;
        private readonly IRepository<GradeLevel, Guid> _gradeLevelRepository;
        private readonly IRepository<Term, Guid> _termRepository;
        private readonly IRepository<CourseStudent, Guid> _courseStudentRepository;
        private readonly IRepository<Entities.Courses.Entities.Course,Guid> _courseRepository;
        private readonly IRepository<Lecture ,Guid> _lectureRepository;
        private readonly IRepository<Chapter,Guid> _chapterRepository;
        public ProfileUserManager(IRepository<Chapter, Guid> chapterRepository, IRepository<Lecture, Guid> lectureRepository, IRepository<Entities.Courses.Entities.Course, Guid> courseRepository, IRepository<CourseStudent, Guid> courseStudentRepository, IRepository<Term, Guid> termRepository, IRepository<GradeLevel, Guid> gradeLevelRepository, IRepository<University, Guid> universityRepository, IRepository<College, Guid> collegeRepository, IRepository<AccountType, Guid> accountTypeRepository, MediaItemManager mediaItemManager, ICurrentUser currentUser, IIdentityUserRepository userRepository) 
        {
            _chapterRepository = chapterRepository;
            _lectureRepository = lectureRepository;
            _courseRepository = courseRepository;
            _courseStudentRepository = courseStudentRepository;
            _termRepository = termRepository;
            _gradeLevelRepository = gradeLevelRepository;
            _universityRepository = universityRepository;
            _collegeRepository = collegeRepository;
            _accountTypeRepository = accountTypeRepository;
            _mediaItemManager = mediaItemManager;
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task<ResponseApi<UserInfoDto>> GetUserInfoAsync(string deviceIp)
        {
            var userInfo = await GetUserDataAsync();
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());
            if (currentUser.GetProperty<string>(SetPropConsts.StudentMobileIP) != string.Empty)
            {
                if (deviceIp.Trim().ToLower() != currentUser.GetProperty<string>(SetPropConsts.StudentMobileIP).Trim().ToLower()) { throw new UserFriendlyException("this devicee not owner this email"); }
            }
            else
            {
                currentUser.SetProperty(SetPropConsts.StudentMobileIP, deviceIp);
                await _userRepository.UpdateAsync(currentUser, autoSave: true);
            }
            return new ResponseApi<UserInfoDto> { Data = userInfo, Success = true, Message = "get user info success" };
        }

        public async Task<ResponseApi<UserInfoDto>> UpdateAllUserDataAsync(UpdateProfielDto input)
        {
            var userInfo = await GetUserDataAsync();
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());
            if(input.Name != null)
            {
                userInfo.Name = input.Name;
                currentUser.Name=input.Name;
            }
            if (input.LogoUrl != null) 
            {
                userInfo.ProfilePictureUrl= input.LogoUrl;
                await _mediaItemManager.UpdateAsync(currentUser.Id ,new CreateUpdateMediaItemDto { RefId=currentUser.Id,Url=input.LogoUrl,IsImage=true}); 
            }
            await _userRepository.UpdateAsync(currentUser);
            return new ResponseApi<UserInfoDto> { Data = userInfo, Success = true, Message = "update all profiel data success" };
        }

        private async Task<UserInfoDto> GetUserDataAsync()
        {
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());
            var media = await _mediaItemManager.GetAsync(currentUser.Id);
            var collegeId = currentUser.GetProperty<Guid?>(SetPropConsts.CollegeId);
            var accountTypeId = currentUser.GetProperty<Guid?>(SetPropConsts.AccountTypeId);
            var universityId = currentUser.GetProperty<Guid?>(SetPropConsts.UniversityId);
            var gradeLevelId = currentUser.GetProperty<Guid?>(SetPropConsts.GradeLevelId);
            var college = await _collegeRepository.FirstOrDefaultAsync(x=>x.Id == collegeId);
            var accountType = await _accountTypeRepository.FirstOrDefaultAsync(x=>x.Id ==accountTypeId);
            var university = await _universityRepository.FirstOrDefaultAsync(x => x.Id == universityId);
            var gradeLevel = await _gradeLevelRepository.FirstOrDefaultAsync(x => x.Id == gradeLevelId);
            var courseJoinCount = await _courseStudentRepository.CountAsync(x=>x.UserId == currentUser.Id && x.IsSubscibe);
            var userInfo = new UserInfoDto
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName,
                Name = currentUser.Name,
                ProfilePictureUrl = media?.Url ?? string.Empty,
                CollegeId = college?.Id ?? null,
                CollegeName = college?.Name ?? string.Empty,
                AccountTypeId = accountType?.Id ?? null,
                AccountTypeKey = accountType?.Key.ToString() ?? string.Empty,
                AccountTypeName = accountType?.Name ?? string.Empty,
                GradeLevelId = gradeLevel?.Id ?? null,
                GradeLevelName = gradeLevel?.Name ?? string.Empty,
                UniversityId = university?.Id ?? null,
                UniversityName = university?.Name ?? string.Empty,
                CourseJoinCount = courseJoinCount,
            };
            return userInfo;
        }

        public async Task<UserInfoDto> GetTeacherProfileAsync()
        {
            var currentUser = await _userRepository.GetAsync(_currentUser.GetId());
            var media = await _mediaItemManager.GetAsync(currentUser.Id);
            var collegeId = currentUser.GetProperty<Guid>(SetPropConsts.CollegeId);
            var accountTypeId = currentUser.GetProperty<Guid>(SetPropConsts.AccountTypeId);
            var universityId = currentUser.GetProperty<Guid>(SetPropConsts.UniversityId);
            var college = await _collegeRepository.FirstOrDefaultAsync(x => x.Id == collegeId);
            var accountType = await _accountTypeRepository.FirstOrDefaultAsync(x => x.Id == accountTypeId);
            var university = await _universityRepository.FirstOrDefaultAsync(x => x.Id == universityId);
            var courseCount = await _courseRepository.CountAsync(x => x.UserId == currentUser.Id );
            var chapterCount = await _chapterRepository.CountAsync(x=>x.CreatorId ==currentUser.Id);
            var lectureCount = await _lectureRepository.CountAsync(x => x.CreatorId == currentUser.Id);
            var userInfo = new UserInfoDto
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName,
                Name = currentUser.Name,
                ProfilePictureUrl = media?.Url ?? UserConsts.DefaultImg,
                CollegeId = college?.Id ?? null,
                CollegeName = college?.Name ?? string.Empty,
                AccountTypeId = accountType?.Id ?? null,
                AccountTypeKey = accountType?.Key.ToString() ?? string.Empty,
                AccountTypeName = accountType?.Name ?? string.Empty,
                UniversityId = university?.Id ?? null,
                UniversityName = university?.Name ?? string.Empty,
                CourseJoinCount = courseCount,
                ChapterCount = chapterCount,
                LectureCount = lectureCount
            };
            return userInfo;
        }
    }
}
