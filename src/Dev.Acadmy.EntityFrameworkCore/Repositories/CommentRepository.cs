using Dev.Acadmy.Dtos.Response.Posts;
using Dev.Acadmy.Entities.Posts.Entities;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class EfCoreCommentRepository : EfCoreRepository<AcadmyDbContext, Comment, Guid>, ICommentRepository
    {
        public EfCoreCommentRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider) { }

        public async Task<(List<CommentDto> items, int totalCount)> GetPagedCommentsAsync(
    Guid postId,
    int pageNumber,
    int pageSize)
        {
            var dbSet = await GetDbSetAsync();
            int skipCount = (Math.Max(pageNumber, 1) - 1) * pageSize;

            // فلترة التعليقات الخاصة بمنشور معين
            var query = dbSet.AsNoTracking()
                .Where(c => c.PostId == postId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(c => c.User) // جلب بيانات كاتب التعليق
                .OrderByDescending(c => c.CreationTime) // الأحدث أولاً
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreationTime = c.CreationTime,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    // يمكن إضافة حقول أخرى مثل عدد الردود إذا كانت موجودة
                })
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
