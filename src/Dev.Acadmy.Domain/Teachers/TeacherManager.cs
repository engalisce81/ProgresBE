using Dev.Acadmy.AccountTypes;
using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.Interfaces;
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
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepo;
        public TeacherManager(IRepository<IdentityUser, Guid> userRepo, IMediaItemRepository mediaItemRepository, IRepository<CourseStudent, Guid> courseStudentRepository, IRepository<Entities.Courses.Entities.Course> courseRepository, IRepository<MediaItem, Guid> mediaItemRepsitory, MediaItemManager mediaItemManager, IRepository<Term, Guid> termRepository, IRepository<GradeLevel, Guid> gradeLevelRepository, IRepository<University, Guid> universityRepository, IRepository<College, Guid> collegeRepository, IRepository<Subject, Guid> subjectRepository, IIdentityRoleRepository roleRepository, IIdentityUserRepository userRepository, IRepository<AccountType, Guid> accountTypeRepository, IdentityUserManager userManager)
        {
            _userRepo = userRepo;
            _mediaItemRepository = mediaItemRepository;
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
            // 1. الحصول على الـ Queryable
            var q = await _userRepo.GetQueryableAsync();

            // 2. فلترة المدرسين فقط داخل القاعدة
            var teacherAccountType = await _accountTypeRepository.FirstOrDefaultAsync(x => x.Key == (int)AccountTypeKey.Teacher);
            if (teacherAccountType != null)
            {
                var pattern = $"%\"{SetPropConsts.AccountTypeId}\":\"{teacherAccountType.Id}\"%";
                q = q.Where(u => EF.Functions.Like((string)(object)u.ExtraProperties, pattern));
            }

            // 3. تطبيق البحث
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchKey = $"%{search.Trim()}%";
                q = q.Where(u => EF.Functions.Like(u.Name, searchKey) || EF.Functions.Like(u.UserName, searchKey));
            }

            var totalCount = await q.CountAsync();

            // 4. جلب الصفحة الحالية
            var users = await q
                .OrderByDescending(x => x.CreationTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 5. تجهيز الـ Dictionaries لجلب الأسماء دفعة واحدة (Bulk Loading)
            var userIds = users.Select(u => u.Id).ToList();

            // جلب الـ IDs الخاصة بالجامعات والكليات من الـ ExtraProperties للمستخدمين الحاليين
            var universityIds = users.Select(u => u.GetProperty<Guid>(SetPropConsts.UniversityId)).Distinct().ToList();
            var collegeIds = users.Select(u => u.GetProperty<Guid>(SetPropConsts.CollegeId)).Distinct().ToList();

            // جلب الأسماء وتحويلها لـ Dictionary (Id -> Name)
            var universityDic =await( await _universityRepository.GetQueryableAsync())
                .Where(x => universityIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var collegeDic = await(await _collegeRepository.GetQueryableAsync())
                .Where(x => collegeIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Name);

            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(userIds);

            // 6. الـ Mapping النهائي
            var teacherDtos = users.Select(user =>
            {
                var uId = user.GetProperty<Guid>(SetPropConsts.UniversityId);
                var cId = user.GetProperty<Guid>(SetPropConsts.CollegeId);

                return new TeacherDto
                {
                    Id = user.Id,
                    FullName = user.Name,
                    UserName = user.UserName,
                    AccountTypeKey = (int)AccountTypeKey.Teacher,
                    UniversityId = uId,
                    UniversityName = universityDic.TryGetValue(uId, out var uName) ? uName : "",
                    CollegeId = cId,
                    CollegeName = collegeDic.TryGetValue(cId, out var cName) ? cName : "",
                    Gender = user.GetProperty<bool>(SetPropConsts.Gender),
                    PhoneNumber = user.GetProperty<string>(SetPropConsts.PhoneNumber) ?? user.PhoneNumber,
                    LogoUrl = mediaItemDic.TryGetValue(user.Id, out var url) ? url : UserConsts.DefaultImg
                };
            }).ToList();

            return new PagedResultDto<TeacherDto>(totalCount, teacherDtos);
        }
        public async Task DeleteAsync(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }

    }
}

