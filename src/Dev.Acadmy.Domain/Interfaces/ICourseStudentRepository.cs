using Dev.Acadmy.Entities.Courses.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseStudentRepository : IRepository<CourseStudent, Guid>
    {
        public Task<List<CourseStudent>> GetListJoinedCoursesByUserIdAsync(Guid userId);
        public Task<List<CourseStudent>> GetListPendingRequestsByUserIdAsync(Guid userId);
        Task<Dictionary<Guid, int>> GetTotalSubscribersPerCourseAsync(IEnumerable<Guid> courseIds);
    }
}
