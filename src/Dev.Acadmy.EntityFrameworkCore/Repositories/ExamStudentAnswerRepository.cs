using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class EfCoreExamStudentAnswerRepository : EfCoreRepository<AcadmyDbContext, ExamStudentAnswer, Guid>, IExamStudentAnswerRepository
    {
        public EfCoreExamStudentAnswerRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        
    }
}
