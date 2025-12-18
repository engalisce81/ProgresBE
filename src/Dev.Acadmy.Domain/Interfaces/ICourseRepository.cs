using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseRepository : IRepository<Entities.Courses.Entities.Course, Guid>
    {
        Task<Entities.Courses.Entities.Course> GetWithHomeDetailesAsync(Guid id);
    }
}
