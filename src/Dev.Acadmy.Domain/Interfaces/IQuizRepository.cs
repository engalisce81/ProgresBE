using Dev.Acadmy.Quizzes;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IQuizRepository:IRepository<Quiz ,Guid>
    {
        Task<Quiz> GetQuizWithQuestionsAsync(Guid quizId);
    }
}
