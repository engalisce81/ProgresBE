using Dev.Acadmy.Quizzes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface IQuizRepository:IRepository<Quiz ,Guid>
    {

    }
}
