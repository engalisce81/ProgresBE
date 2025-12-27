using Dev.Acadmy.Dtos.Response.Posts;
using Dev.Acadmy.Entities.Posts.Entities;
using Dev.Acadmy.EntityFrameworkCore;
using Dev.Acadmy.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Dev.Acadmy.Repositories
{
    public class PostRepository : EfCoreRepository<AcadmyDbContext, Post, Guid>, IPostRepository
    {
        public PostRepository(IDbContextProvider<AcadmyDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<PostDto> GetDetailedPostAsync(Guid id)
        {
            var dbSet = await GetDbSetAsync();

            return await dbSet
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    IsGeneral = p.IsGeneral,
                    AuthorName = p.User.UserName,
                    CreationTime = p.CreationTime,
                    TotalCommentsCount = p.Comments.Count,
                    ReactionsSummaries = p.Reactions
                        .GroupBy(r => r.Type)
                        .Select(g => new ReactionSummaryDto
                        {
                            Type = (int)g.Key,
                            Count = g.Count()
                        }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(List<PostDto> items, int totalCount)> GetDetailedPostsAsync(bool? isGeneral, int pageNumber, int pageSize)
        {
            var dbSet = await GetDbSetAsync();

            // 1. بناء الاستعلام الأساسي مع الفلترة
            var query = dbSet
                .AsNoTracking()
                .WhereIf(isGeneral.HasValue, x => x.IsGeneral == isGeneral.Value);

            // 2. حساب العدد الإجمالي
            var totalCount = await query.CountAsync();

            // 3. حساب عدد السجلات التي سيتم تخطيها (Skip)
            // نضمن أن الصفحة لا تقل عن 1
            int skipCount = (Math.Max(pageNumber, 1) - 1) * pageSize;

            // 4. تنفيذ الـ Projection والترتيب والـ Paging
            var items = await query
                .AsNoTracking() // مهم جداً للأداء في عمليات القراءة
                .Include(p => p.User) // لضمان جلب بيانات صاحب المنشور
                .Include(p => p.Comments) // لضمان الوصول لمجموعة التعليقات
                .Include(p => p.Reactions) // لضمان الوصول لمجموعة التفاعلات
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    IsGeneral = p.IsGeneral,
                    AuthorName = p.User.UserName,
                    CreationTime = p.CreationTime,
                    TotalCommentsCount = p.Comments.Count,
                    ReactionsSummaries = p.Reactions
                        .GroupBy(r => r.Type)
                        .Select(g => new ReactionSummaryDto
                        {
                            // g.Key هو الـ Enum الممثل لنوع التفاعل
                            Type = (int)g.Key,
                            Count = g.Count()
                        }).ToList()
                })
                // الترتيب بناءً على مجموع التفاعلات + التعليقات
                .OrderByDescending(x => x.ReactionsSummaries.Sum(s => s.Count) + x.TotalCommentsCount)
                .ThenByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<PostDto> items, int totalCount)> GetDetailedPostsAsync(Guid userId, int pageNumber, int pageSize)
        {
            var dbSet = await GetDbSetAsync();

            // 1. بناء الاستعلام الأساسي مع الفلترة
            var query = dbSet
                .AsNoTracking().Where(p => p.UserId == userId); // منشوراتي فق
            // 2. حساب العدد الإجمالي
            var totalCount = await query.CountAsync();

            // 3. حساب عدد السجلات التي سيتم تخطيها (Skip)
            // نضمن أن الصفحة لا تقل عن 1
            int skipCount = (Math.Max(pageNumber, 1) - 1) * pageSize;

            // 4. تنفيذ الـ Projection والترتيب والـ Paging
            var items = await query
                .AsNoTracking() // مهم جداً للأداء في عمليات القراءة
                .Include(p => p.User) // لضمان جلب بيانات صاحب المنشور
                .Include(p => p.Comments) // لضمان الوصول لمجموعة التعليقات
                .Include(p => p.Reactions) // لضمان الوصول لمجموعة التفاعلات
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    IsGeneral = p.IsGeneral,
                    AuthorName = p.User.UserName,
                    CreationTime = p.CreationTime,
                    TotalCommentsCount = p.Comments.Count,
                    ReactionsSummaries = p.Reactions
                        .GroupBy(r => r.Type)
                        .Select(g => new ReactionSummaryDto
                        {
                            // g.Key هو الـ Enum الممثل لنوع التفاعل
                            Type = (int)g.Key,
                            Count = g.Count()
                        }).ToList()
                })
                // الترتيب بناءً على مجموع التفاعلات + التعليقات
                .OrderByDescending(x => x.ReactionsSummaries.Sum(s => s.Count) + x.TotalCommentsCount)
                .ThenByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
