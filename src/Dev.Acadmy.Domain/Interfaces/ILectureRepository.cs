using Dev.Acadmy.Lectures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ILectureRepository : IRepository<Lecture, Guid>
    {
        Task<Dictionary<Guid, LectureTryDto>> GetLecturesStatusAsync(Guid userId, List<Guid> lectureIds, List<Guid> quizIds);

    }
}
