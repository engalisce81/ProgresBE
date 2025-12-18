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
    public class CoreCourseRepository
    : EfCoreRepository<AcadmyDbContext, Entities.Courses.Entities.Course, Guid>, ICourseRepository
    {
        public CoreCourseRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<Entities.Courses.Entities.Course> GetWithHomeDetailesAsync(Guid id)
        {
            var dbSet = await GetDbSetAsync();

            return await dbSet
                .Include(c => c.User)
                .Include(x => x.Subject)
                .Include(x => x.CourseInfos)
                .Include(c => c.College)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lectures) 
                .Include(c => c.Feedbacks)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
