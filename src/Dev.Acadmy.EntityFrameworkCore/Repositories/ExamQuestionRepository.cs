using Dev.Acadmy.Chapters;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Exams;
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
    internal class ExamQuestionRepository : EfCoreRepository<AcadmyDbContext, ExamQuestion, Guid>, IExamQuestionRepository
    {
        public ExamQuestionRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider) { }

        public async Task<List<ExamQuestion>> GetQuestionsByExamIdAsync(Guid examId)
        {
            return await (await GetQueryableAsync())
                .Include(x => x.Question)
                    .ThenInclude(q => q.QuestionAnswers)
                .Include(x => x.Question)
                    .ThenInclude(q => q.QuestionType)
                .Where(x => x.ExamId == examId)
                .ToListAsync();
        }
    }
}
