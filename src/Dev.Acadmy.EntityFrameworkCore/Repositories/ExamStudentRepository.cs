using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Interfaces.Dev.Acadmy.Exams;
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
    public class ExamStudentRepository : EfCoreRepository<AcadmyDbContext, ExamStudent, Guid>, IExamStudentRepository
    {
        public ExamStudentRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        // تنفيذ الميثود المخصصة لجلب المحاولة مع بيانات الامتحان والمستخدم
        public async Task<ExamStudent> GetWithDetailsAsync(Guid examId, Guid userId)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Include(x => x.Exam) // جلب بيانات الامتحان
                .Include(x => x.User) // جلب بيانات الطالب
                .FirstOrDefaultAsync(x => x.ExamId == examId && x.UserId == userId);
        }
    }
}
