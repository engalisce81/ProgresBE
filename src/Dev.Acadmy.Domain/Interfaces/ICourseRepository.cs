using Dev.Acadmy.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseRepository : IRepository<Entities.Courses.Entities.Course, Guid>
    {
        Task<Entities.Courses.Entities.Course> GetWithHomeDetailesAsync(Guid id);

        Task<(List<Entities.Courses.Entities.Course> Items, long TotalCount)> GetListWithDetailsAsync(
        int skipCount,
        int maxResultCount,
        string? search,
        CourseType type,
        Guid? userId = null,
        bool isAdmin = false
    );
    }
}
