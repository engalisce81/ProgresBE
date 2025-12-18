using Dev.Acadmy.AccountTypes;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.LookUp;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Response;
using Dev.Acadmy.Universites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Teachers
{
    public class TeacherManager : DomainService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly IdentityUserManager _userManager;
        private readonly IRepository<AccountType, Guid> _accountTypeRepository;
        private readonly IIdentityRoleRepository _roleRepository;
        private readonly IRepository<Subject, Guid> _subjectRepository;
        private readonly IRepository<College, Guid> _collegeRepository;
        private readonly IRepository<University, Guid> _universityRepository;
        private readonly IRepository<GradeLevel, Guid> _gradeLevelRepository;
        private readonly IRepository<Term, Guid> _termRepository;
        private readonly MediaItemManager _mediaItemManager;
        private readonly IRepository<MediaItem, Guid> _mediaItemRepsitory;
        private readonly IRepository<Entities.Courses.Entities.Course> _courseRepository;
        private readonly IRepository<CourseStudent, Guid> _courseStudentRepository;
        public TeacherManager(IRepository<CourseStudent, Guid> courseStudentRepository, IRepository<Entities.Courses.Entities.Course> courseRepository, IRepository<MediaItem, Guid> mediaItemRepsitory, MediaItemManager mediaItemManager, IRepository<Term, Guid> termRepository, IRepository<GradeLevel, Guid> gradeLevelRepository, IRepository<University, Guid> universityRepository, IRepository<College, Guid> collegeRepository, IRepository<Subject, Guid> subjectRepository, IIdentityRoleRepository roleRepository, IIdentityUserRepository userRepository, IRepository<AccountType, Guid> accountTypeRepository, IdentityUserManager userManager)
        {
            _courseStudentRepository = courseStudentRepository; 
            _courseRepository = courseRepository;
            _mediaItemRepsitory = mediaItemRepsitory;
            _mediaItemManager = mediaItemManager;
            _termRepository = termRepository;
            _gradeLevelRepository = gradeLevelRepository;
            _universityRepository = universityRepository;
            _collegeRepository = collegeRepository;
            _subjectRepository = subjectRepository;
            _roleRepository = roleRepository;
            _userManager = userManager;
            _accountTypeRepository = accountTypeRepository;
            _userRepository = userRepository;
        }


        public async Task<ResponseApi<LookupDto>> CreateTeacherAsync(CreateUpdateTeacherDto input)
        {
            await CheckEntity(input);
            if (await _userRepository.FindByNormalizedEmailAsync(input.UserName.ToUpper()) != null) throw new UserFriendlyException("The Email or User Name Already Exist");
            var user = new IdentityUser(Guid.NewGuid(), input.UserName, input.UserName);
            var accountType = await _accountTypeRepository.FirstOrDefaultAsync(x => x.Key == input.AccountTypeKey);
            if (accountType == null) throw new UserFriendlyException("Account Type Not Found");
            var role = await GetRole(accountType.Id);
            user.SetProperty(SetPropConsts.AccountTypeId, accountType.Id);
            user.Name = input.FullName;
            user.SetProperty(SetPropConsts.CollegeId, input.CollegeId);
            user.SetProperty(SetPropConsts.Gender, input.Gender);
            user.SetProperty(SetPropConsts.UniversityId, input.UniversityId);
            user.SetProperty(SetPropConsts.PhoneNumber, input.PhoneNumber);
            var currentTerm = await _termRepository.FirstOrDefaultAsync(x => x.IsActive);
            if (currentTerm != null) user.SetProperty(SetPropConsts.TermId, currentTerm.Id);
            user.SetIsActive(true);
            var result = await _userManager.CreateAsync(user, input.Password);
            if (result.Succeeded)
            {
                if (role != null)
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                    if (!result.Succeeded) return new ResponseApi<LookupDto> { Data = null, Success = false, Message = result.Errors.FirstOrDefault()?.Description ?? "" };
                    else
                    {
                        var lookupDto = new LookupDto { Id = user.Id, Name = input.FullName };
                        return new ResponseApi<LookupDto> { Data = lookupDto, Success = true, Message = "Register Success" };
                    }
                }
                else throw new UserFriendlyException("Role Not Found");
            }
            else throw new UserFriendlyException("Can't Create This Account");
        }
        private async Task<IdentityRole> GetRole(Guid accountTypeId)
        {
            var accountType = await _accountTypeRepository.GetAsync(accountTypeId);
            if (accountType == null) new UserFriendlyException($"Not Found Account Type With Id{accountTypeId}");
            if (accountType.Key == (int)AccountTypeKey.Teacher) return await _roleRepository.FindByNormalizedNameAsync(RoleConsts.Teacher.ToUpperInvariant());
            else return await _roleRepository.FindByNormalizedNameAsync(RoleConsts.Teacher.ToUpperInvariant());
        }

        private async Task CheckEntity(CreateUpdateTeacherDto input)
        {
            var university = await _universityRepository.GetAsync(input.UniversityId);
            var college = await _collegeRepository.GetAsync(input.CollegeId);
        }


        public async Task<ResponseApi<LookupDto>> UpdateAsync(Guid userId, CreateUpdateTeacherDto input)
        {
            // 🟢 1. التحقق من صحة البيانات
            await CheckEntity(input);

            // 🟢 2. الحصول على المستخدم الحالي
            var user = await _userRepository.FindAsync(userId);
            if (user == null)
                throw new UserFriendlyException("User Not Found");

            // 🟢 3. التحقق من عدم وجود بريد إلكتروني أو اسم مستخدم مكرر (لبقية المستخدمين)
            var existingUser = await _userRepository.FindByNormalizedEmailAsync(input.UserName.ToUpper());
            if (existingUser != null && existingUser.Id != userId)
                throw new UserFriendlyException("The Email or User Name Already Exist");

            // 🟢 4. التحقق من نوع الحساب
            var accountType = await _accountTypeRepository.FirstOrDefaultAsync(x => x.Key == input.AccountTypeKey);
            if (accountType == null)
                throw new UserFriendlyException("Account Type Not Found");

            // 🟢 5. تحديث الخصائص الأساسية
            await _userManager.SetUserNameAsync(user, input.UserName);
            await _userManager.SetEmailAsync(user, input.UserName);
            user.Name = input.FullName;
            user.SetProperty(SetPropConsts.AccountTypeId, accountType.Id);
            user.SetProperty(SetPropConsts.CollegeId, input.CollegeId);
            user.SetProperty(SetPropConsts.Gender, input.Gender);
            user.SetProperty(SetPropConsts.UniversityId, input.UniversityId);
            user.SetProperty(SetPropConsts.PhoneNumber, input.PhoneNumber);
            // 🟢 6. تحديث خصائص إضافية حسب نوع الحساب
            if (accountType.Key == (int)AccountTypeKey.Teacher)
            {
                var currentTerm = await _termRepository.FirstOrDefaultAsync(x => x.IsActive);
                if (currentTerm != null) user.SetProperty(SetPropConsts.TermId, currentTerm.Id);
            }
            // 🟢 7. تحديث حالة المستخدم
            user.SetIsActive(true);

            // 🟢 8. تحديث المستخدم في قاعدة البيانات
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new UserFriendlyException(updateResult.Errors.FirstOrDefault()?.Description ?? "Failed To Update User");

            // 🟢 9. التحقق من الـ Role الحالي وتحديثه لو تغيّر نوع الحساب
            var userRoles = await _userManager.GetRolesAsync(user);
            var currentRoleName = userRoles.FirstOrDefault();

            var newRole = await GetRole(accountType.Id);
            if (newRole == null)
                throw new UserFriendlyException("Role Not Found");

            if (currentRoleName != newRole.Name)
            {
                if (currentRoleName != null)
                    await _userManager.RemoveFromRoleAsync(user, currentRoleName);
                await _userManager.AddToRoleAsync(user, newRole.Name);
            }

            // 🟢 10. إرجاع النتيجة
            var lookupDto = new LookupDto { Id = user.Id, Name = user.Name };
            return new ResponseApi<LookupDto> { Data = lookupDto, Success = true, Message = "User Updated Successfully" };
        }

        public async Task<ResponseApi<TeacherDto>> GetAsync(Guid userId)
        {
            // 🟢 1. الحصول على المستخدم
            var user = await _userRepository.FindAsync(userId);
            if (user == null)
                throw new UserFriendlyException("User Not Found");

            // 🟢 2. قراءة نوع الحساب
            var accountTypeId = user.GetProperty<Guid?>(SetPropConsts.AccountTypeId);
            var accountType = accountTypeId.HasValue
                ? await _accountTypeRepository.FindAsync(accountTypeId.Value)
                : null;

            // 🟢 3. تعبئة البيانات في DTO
            var dto = new TeacherDto
            {
                Id = user.Id,
                FullName = user.Name,
                UserName = user.UserName,
                AccountTypeKey = accountType?.Key ?? 0,
                CollegeId = user.GetProperty<Guid>(SetPropConsts.CollegeId),
                UniversityId = user.GetProperty<Guid>(SetPropConsts.UniversityId),
                Gender = user.GetProperty<bool>(SetPropConsts.Gender),
                PhoneNumber = user.GetProperty<string>(SetPropConsts.PhoneNumber)
            };

            // 🟢 4. إرجاع النتيجة
            return new ResponseApi<TeacherDto>
            {
                Data = dto,
                Success = true,
                Message = "User Retrieved Successfully"
            };
        }

        public async Task<PagedResultDto<TeacherDto>> GetTeacherListAsync(
         int pageNumber = 1,
         int pageSize = 10,
         string? search = null)
        {
            // 🟢 1. جلب جميع المستخدمين من المستودع
            var users = await _userRepository.GetListAsync();

            var resultList = new List<TeacherDto>();

            // 🟢 2. المرور على كل مستخدم
            foreach (var user in users)
            {
                var accountTypeId = user.GetProperty<Guid?>(SetPropConsts.AccountTypeId);
                if (!accountTypeId.HasValue)
                    continue;

                var accountType = await _accountTypeRepository.FindAsync(accountTypeId.Value);
                if (accountType == null || accountType.Key != (int)AccountTypeKey.Teacher)
                    continue;

                // 🟢 بناء الـ DTO
                var dto = new TeacherDto
                {
                    Id = user.Id,
                    FullName = user.Name,
                    UserName = user.UserName,
                    AccountTypeKey = accountType.Key,
                    CollegeId = user.GetProperty<Guid>(SetPropConsts.CollegeId),
                    UniversityId = user.GetProperty<Guid>(SetPropConsts.UniversityId),
                    Gender = user.GetProperty<bool>(SetPropConsts.Gender),
                    PhoneNumber = user.GetProperty<string>(SetPropConsts.PhoneNumber),
                    LogoUrl = (_mediaItemManager.GetAsync(user.Id).Result)?.Url ?? UserConsts.DefaultImg
                };
                resultList.Add(dto);
            }

            // 🟢 3. تطبيق البحث (لو موجود)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                resultList = resultList
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.FullName) && x.FullName.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(x.UserName) && x.UserName.ToLower().Contains(search))
                    )
                    .ToList();
            }

            // 🟢 4. إجمالي السجلات بعد الفلترة
            var totalCount = resultList.Count;

            // 🟢 5. تطبيق الـ Pagination
            var pagedResult = resultList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 🟢 6. إرجاع النتيجة بصيغة ABP القياسية
            return new PagedResultDto<TeacherDto>(
                totalCount,
                pagedResult
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }

    }
}

