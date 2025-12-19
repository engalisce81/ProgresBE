using Dev.Acadmy;
using Dev.Acadmy.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Course.Data.Seeds
{
    public class AsignPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IIdentityRoleRepository _roleRepository;
        private readonly IdentityRoleManager _roleManager;
        private readonly IPermissionManager _permissionManager;
        public AsignPermissionDataSeedContributor(IIdentityRoleRepository roleRepository, IdentityRoleManager roleManager, IPermissionManager permissionManager)
        {
            _roleRepository = roleRepository;
            _roleManager = roleManager;
            _permissionManager = permissionManager;
        }
        public async Task SeedAsync(DataSeedContext context)
        {
            await StudentFeedbackPermission();
            await SetAdvertisementPermissionsAsync();
            await SetCourseStudentsPermissionsAsync();
        }

       

        public async Task StudentFeedbackPermission()
        {
            // 1. صلاحيات الطالب (Student)
            // الطالب يحتاج صلاحية العرض، الإنشاء، والتعديل (لتعديل تقييمه الخاص)
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Default, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.View, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Create, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Edit, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Delete, true);


            // 2. صلاحيات المدرس أو الإدمن (Teacher / Admin)
            // المدرس يحتاج كل الصلاحيات بما فيها "القبول" و "الحذف"
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Default, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.View, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Create, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Edit, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Delete, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Accept, true);
        }

        public async Task SetCourseStudentsPermissionsAsync()
        {
            // --- صلاحيات الطالب (Student) ---
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseStudents.Default, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseStudents.Create, true);

            // --- صلاحيات المدرس (Teacher) ---
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseStudents.Default, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseStudents.View, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseStudents.Create, true);
        }
        public async Task SetAdvertisementPermissionsAsync()
        {
           
            // --- صلاحيات المدرس (Teacher) ---
            // المدرس غالباً يحتاج فقط لرؤية الإعلانات (View)
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.Advertisements.Default, true);

            // --- صلاحيات الطالب (Student) ---
            // الطالب يحتاج لرؤية الإعلانات المنشورة له
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.Advertisements.Default, true);
        }

    }
}
