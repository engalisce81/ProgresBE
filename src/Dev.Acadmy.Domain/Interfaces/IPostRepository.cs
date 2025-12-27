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
    public interface IPostRepository : IRepository<Post, Guid>
    {
        // دالة لجلب المنشور مع التعليقات والتفاعلات وبيانات المستخدمين في طلب واحد
        Task<PostDto> GetDetailedPostAsync(Guid id);
        Task<(List<PostDto> items, int totalCount)> GetDetailedPostsAsync(bool? isGeneral,int pageNumber,int pageSize);
        Task<(List<PostDto> items, int totalCount)> GetDetailedPostsAsync(Guid userId, int pageNumber, int pageSize);
    }
}
