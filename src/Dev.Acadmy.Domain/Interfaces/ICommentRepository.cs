using Dev.Acadmy.Dtos.Response.Posts;
using Dev.Acadmy.Entities.Posts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Dev.Acadmy.Interfaces
{
    public interface ICommentRepository : IRepository<Comment, Guid>
    {
        Task<(List<CommentDto> items, int totalCount)> GetPagedCommentsAsync(Guid postId, int pageNumber, int pageSize);

    }
}
