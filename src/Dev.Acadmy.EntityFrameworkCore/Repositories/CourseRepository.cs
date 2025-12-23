using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Enums;
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

        public async Task<(List<Entities.Courses.Entities.Course> Items, long TotalCount)> GetListWithDetailsAsync(
    int skipCount,
    int maxResultCount,
    string? search,
    CourseType type,
    Guid? userId = null,
    bool isAdmin = false)
        {
            var query = await GetQueryableAsync();

            // تطبيق الفلاتر (نفس المنطق السابق)
            query = query.WhereIf(!string.IsNullOrWhiteSpace(search), x => x.Name.Contains(search));

            query = type switch
            {
                CourseType.Quiz => query.Where(x => x.IsQuiz),
                CourseType.Pdf => query.Where(x => x.IsPdf),
                _ => query
            };

            if (!isAdmin && userId.HasValue)
            {
                query = query.Where(x => x.UserId == userId.Value);
            }

            // 1. حساب الإجمالي قبل عمل Skip و Take
            var totalCount = await query.LongCountAsync();

            // 2. جلب البيانات مع الـ Includes والـ Pagination
            var items = await query.Include(x => x.College)
                                   .Include(x => x.Exams)
                                   .Include(x => x.QuestionBanks)
                                   .WhereIf(!isAdmin, x => true) // كود توضيحي لو أردت إضافة شروط إضافية
                                   .OrderByDescending(x => x.CreationTime)
                                   .PageBy(skipCount, maxResultCount)
                                   .ToListAsync();

            return (items, totalCount);
        }
    }
}
