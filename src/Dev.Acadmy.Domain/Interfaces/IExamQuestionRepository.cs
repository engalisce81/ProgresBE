using Dev.Acadmy.Exams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IExamQuestionRepository : IRepository<ExamQuestion, Guid>
    {
        Task<List<ExamQuestion>> GetQuestionsByExamIdAsync(Guid examId);
    }
}
