using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Quizzes;
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
    // في طبقة الـ EntityFrameworkCore
    public class QuizRepository : EfCoreRepository<AcadmyDbContext, Quiz, Guid>, IQuizRepository
    {
        public QuizRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
        public async Task<Quiz> GetQuizWithQuestionsAsync(Guid quizId)
        {
            return await (await GetQueryableAsync())
                .Include(q => q.Questions)
                    .ThenInclude(q => q.QuestionAnswers)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.QuestionType)
                .Include(q => q.Lecture) // ستحتاجها لجلب عدد المحاولات (QuizTryCount)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }
    }
}
