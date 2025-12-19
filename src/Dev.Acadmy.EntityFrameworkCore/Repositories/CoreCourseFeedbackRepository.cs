using Dev.Acadmy.Dtos.Response.Courses;
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
    public class CoreCourseFeedbackRepository : EfCoreRepository<AcadmyDbContext, CourseFeedback, Guid>, ICourseFeedbackRepository
    {
        public CoreCourseFeedbackRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider) { }

        public async Task<List<FeedbackDto>> GetListFeedbacksByCourseIdAsync(
     Guid courseId,
     int pageNumber,
     int pageSize,
     bool isAccept)
        {
            var dbContext = await GetDbContextAsync();

            return await dbContext.Set<CourseFeedback>()
                .AsNoTracking() // لتحسين الأداء لأننا نقوم بالقراءة فقط
                .Include(x => x.User)
                // 1. الفلترة بناءً على الكورس وحالة القبول
                .Where(x => x.CourseId == courseId && x.IsAccept == isAccept)
                // الفلترة: التقييمات المقبولة فقط لهذا الكورس المحد
                .OrderByDescending(x => x.CreationTime)
                // 3. الترقيم (Pagination)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                // 4. الإسقاط (Projection) لتحويل البيانات إلى DTO
                .Select(f => new FeedbackDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    UserName = f.User.Name, // تأكد أن علاقة الـ User معرفة بشكل صحيح
                    LogoUrl = ""
                })
                .ToListAsync();
        }

        public async Task<List<FeedbackDto>> GetListSumFeedByCourseIdAsync(Guid courseId, int numberFeedback)
        {
            var dbContext = await GetDbContextAsync();

            return await dbContext.Set<CourseFeedback>()
                .Include(x => x.User)
                // الفلترة: التقييمات المقبولة فقط لهذا الكورس المحدد
                .Where(x => x.CourseId == courseId && x.IsAccept)
                .OrderByDescending(x => x.CreationTime)
                // تحديد عدد السجلات المطلوبة
                .Take(numberFeedback)
                .Select(f => new FeedbackDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    UserName = f.User.Name,
                    LogoUrl = "" // سيتم ملؤها في الـ Application Service من الـ Media Repository
                })
                .ToListAsync();
        }
    }
}
