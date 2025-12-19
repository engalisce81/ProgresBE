using Dev.Acadmy.Dtos.Response.Courses;
using Dev.Acadmy.Entities.Courses.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICourseFeedbackRepository : IRepository<CourseFeedback, Guid>
    {
        Task<List<FeedbackDto>> GetListSumFeedByCourseIdAsync(Guid courseId, int numberFeedback);
        Task<List<FeedbackDto>> GetListFeedbacksByCourseIdAsync(Guid courseId, int pageNumber, int pageSize, bool isAccept);

    }
}
