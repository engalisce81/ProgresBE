using Dev.Acadmy.Courses;
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
    public class CourseInfoRepository : EfCoreRepository<AcadmyDbContext, CourseInfo , Guid> , ICourseInfoRepository
    {
        public CourseInfoRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
        public async Task DeleteByCourseIdAsync(Guid courseId)
    => await (await GetDbSetAsync()).Where(x => x.CourseId == courseId).ExecuteDeleteAsync();

    }
}
