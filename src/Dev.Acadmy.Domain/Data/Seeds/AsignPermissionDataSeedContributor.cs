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
            await StudentCoursePermission();
        }

        public async Task StudentCoursePermission()
        {
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.Courses.Create, true);
            
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.Courses.Edit, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.Courses.View, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.Courses.Delete, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher , AcadmyPermissions.Courses.Default, true);
        }

        public async Task StudentFeedbackPermission()
        {
            // 1. صلاحيات الطالب (Student)
            // الطالب يحتاج صلاحية العرض، الإنشاء، والتعديل (لتعديل تقييمه الخاص)
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Default, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.View, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Create, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Student, AcadmyPermissions.CourseFeedbacks.Edit, true);

            // 2. صلاحيات المدرس أو الإدمن (Teacher / Admin)
            // المدرس يحتاج كل الصلاحيات بما فيها "القبول" و "الحذف"
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Default, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.View, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Create, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Edit, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Delete, true);
            await _permissionManager.SetForRoleAsync(RoleConsts.Teacher, AcadmyPermissions.CourseFeedbacks.Accept, true);
        }

    }
}
