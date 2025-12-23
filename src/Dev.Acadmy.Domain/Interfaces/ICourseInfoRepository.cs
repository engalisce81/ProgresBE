using Dev.Acadmy.Courses;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseInfoRepository : IRepository<CourseInfo, Guid>
    {
        Task DeleteByCourseIdAsync(Guid courseId);
    }
}
