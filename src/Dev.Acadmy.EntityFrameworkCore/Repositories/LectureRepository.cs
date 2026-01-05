using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.Lectures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class LectureRepository : EfCoreRepository<AcadmyDbContext, Lecture, Guid>, ILectureRepository
    {


        public LectureRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public Task<Dictionary<Guid, LectureTryDto>> GetLecturesStatusAsync(Guid userId, List<Guid> lectureIds, List<Guid> quizIds)
        {
            throw new NotImplementedException();
        }
    }
}
