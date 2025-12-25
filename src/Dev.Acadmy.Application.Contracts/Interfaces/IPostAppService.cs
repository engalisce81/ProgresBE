using Dev.Acadmy.Dtos.Request.Posts;
using Dev.Acadmy.Dtos.Response.Posts;
using Dev.Acadmy.Response;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Dev.Acadmy.Posts
{
    public interface IPostAppService : IApplicationService
    {
        // عمليات المنشورات (Posts)
        Task<ResponseApi<PostDto>> GetAsync(Guid id);

        Task<PagedResultDto<PostDto>> GetListPostAsync(bool? isGeneral, int pageNumber, int pageSize);

        Task<PagedResultDto<PostDto>> GetMyPostsAsync(int pageNumber = 1, int pageSize = 10);

        Task<ResponseApi<PostDto>> CreateAsync(CreateUpdatePostDto input);

        Task<ResponseApi<PostDto>> UpdateAsync(Guid id, CreateUpdatePostDto input);

        Task DeleteAsync(Guid id);

        // عمليات التعليقات (Comments)
        Task<ResponseApi<CommentDto>> AddCommentAsync(CreateUpdateCommentDto input);

        Task<PagedResultDto<CommentDto>> GetListCommentAsync(Guid postId, int pageNumber, int pageSize);

        Task<ResponseApi<CommentDto>> UpdateCommentAsync(Guid commentId, string newText);

        Task DeleteCommentAsync(Guid commentId);

        // عمليات التفاعلات (Reactions)
        Task ToggleReactionAsync(CreateUpdateReactionDto input);
    }
}