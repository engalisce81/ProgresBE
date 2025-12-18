using Dev.Acadmy.Entities.Courses.Entities;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class CoreCourseStudentRepository
     : EfCoreRepository<AcadmyDbContext, CourseStudent, Guid>, ICourseStudentRepository
    {
        public CoreCourseStudentRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        // جلب الكورسات المقبولة (IsSubscibe = true)
        public async Task<List<CourseStudent>> GetListJoinedCoursesByUserIdAsync(Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Include(x => x.Course) // مهم لجلب بيانات الكورس
                .Where(x => x.UserId == userId && x.IsSubscibe)
                .ToListAsync();
        }

        // جلب الطلبات المعلقة (IsSubscibe = false)
        public async Task<List<CourseStudent>> GetListPendingRequestsByUserIdAsync(Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Include(x => x.Course)
                .Where(x => x.UserId == userId && !x.IsSubscibe)
                .ToListAsync();
        }
    }
}
