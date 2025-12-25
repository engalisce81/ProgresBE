using AutoMapper;
using Dev.Acadmy.Dtos.Request.Posts;
using Dev.Acadmy.Dtos.Response.Posts;
using Dev.Acadmy.Entities.Posts.Entities;
using Dev.Acadmy.Entities.Posts.Managers;
using Dev.Acadmy.Enums;
using Dev.Acadmy.Interfaces;
using Dev.Acadmy.MediaItems;
using Dev.Acadmy.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Dev.Acadmy.Posts
{
    [Authorize] // حماية الكلاس بالكامل
    public class PostAppService : ApplicationService, IPostAppService
    {
        private readonly IMapper _mapper;
        private readonly IMediaItemRepository _mediaItemRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IReactionRepository _reactionRepository;
        private readonly IPostRepository _postRepository;
        private readonly PostManager _postManager;
        private readonly MediaItemManager _mediaItemManager;

        public PostAppService(
            IMapper mapper,
            IMediaItemRepository mediaItemRepository,
            ICommentRepository commentRepository,
            IReactionRepository reactionRepository,
            IPostRepository postRepository,
            PostManager postManager,
            MediaItemManager mediaItemManager)
        {
            _mapper = mapper;
            _mediaItemRepository = mediaItemRepository;
            _commentRepository = commentRepository;
            _reactionRepository = reactionRepository;
            _postRepository = postRepository;
            _postManager = postManager;
            _mediaItemManager = mediaItemManager;
        }

        #region Posts

        public async Task<ResponseApi<PostDto>> GetAsync(Guid id)
        {
            var postDto = await _postRepository.GetDetailedPostAsync(id);

            if (postDto == null)
            {
                return new ResponseApi<PostDto> { Success = false, Message = "المنشور غير موجود" };
            }

            postDto.LogoUrl = await _mediaItemRepository.GetUrlByEntityIdAsync(id);

            return new ResponseApi<PostDto> { Data = postDto, Success = true };
        }

        public async Task<PagedResultDto<PostDto>> GetListPostAsync(bool? isGeneral, int pageNumber, int pageSize)
        {
            var (posts, totalCount) = await _postRepository.GetDetailedPostsAsync(isGeneral, pageNumber, pageSize);
            var postIds = posts.Select(p => p.Id).ToList();
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(postIds);

            foreach (var post in posts)
            {
                if (mediaItemDic.TryGetValue(post.Id, out var url))
                    post.LogoUrl = url;
            }

            return new PagedResultDto<PostDto>(totalCount, posts);
        }

        public async Task<PagedResultDto<PostDto>> GetMyPostsAsync(int pageNumber, int pageSize)
        {
            var userId = CurrentUser.GetId();
            var (posts, totalCount) = await _postRepository.GetDetailedPostsAsync(userId, pageNumber, pageSize);

            if (totalCount == 0) return new PagedResultDto<PostDto>(0, new List<PostDto>());

            var postIds = posts.Select(p => p.Id).ToList();
            var mediaItemDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(postIds);

            foreach (var post in posts)
            {
                if (mediaItemDic.TryGetValue(post.Id, out var url))
                    post.LogoUrl = url;
            }

            return new PagedResultDto<PostDto>(totalCount, posts);
        }

        public async Task<ResponseApi<PostDto>> CreateAsync(CreateUpdatePostDto input)
        {
            var post = await _postManager.CreateAsync(input.Title, input.Content, input.IsGeneral, CurrentUser.GetId());
            var result = await _postRepository.InsertAsync(post, autoSave: true);

            string url = string.Empty;
            if (input.File != null)
                url = await _mediaItemRepository.InsertAsync(input.File, result.Id);

            // استخدام _mapper بدلاً من ObjectMapper
            var dto = _mapper.Map<PostDto>(result);
            dto.LogoUrl = url;

            return new ResponseApi<PostDto> { Data = dto, Success = true, Message = "تم النشر بنجاح" };
        }

        public async Task<ResponseApi<PostDto>> UpdateAsync(Guid id, CreateUpdatePostDto input)
        {
            var post = await _postRepository.GetAsync(id);
            await _postManager.UpdateAsync(post, input.Title, input.Content, input.IsGeneral, CurrentUser.GetId());

            var result = await _postRepository.UpdateAsync(post, autoSave: true);
            var url = await _mediaItemManager.UpdateAsync(input.File, id);

            // استخدام _mapper بدلاً من ObjectMapper
            var dto = _mapper.Map< PostDto>(result);
            dto.LogoUrl = url;

            return new ResponseApi<PostDto> { Data = dto, Success = true, Message = "تم التحديث بنجاح" };
        }

        public async Task DeleteAsync(Guid id)
        {
            var post = await _postRepository.GetAsync(id);
            await _postRepository.DeleteAsync(post);
        }

        #endregion

        #region Comments

        public async Task<ResponseApi<CommentDto>> AddCommentAsync(CreateUpdateCommentDto input)
        {
            var comment = new Comment { PostId = input.PostId, Text = input.Text, UserId = CurrentUser.GetId() };
            var insertedComment = await _commentRepository.InsertAsync(comment, autoSave: true);

            // استخدام _mapper بدلاً من ObjectMapper
            var dto = _mapper.Map<Comment, CommentDto>(insertedComment);
            dto.UserName = CurrentUser.UserName;
            dto.LogoUrl = await _mediaItemRepository.GetUrlByEntityIdAsync(CurrentUser.GetId());

            return new ResponseApi<CommentDto> { Data = dto, Success = true };
        }

        public async Task<PagedResultDto<CommentDto>> GetListCommentAsync(Guid postId, int pageNumber, int pageSize)
        {
            var (comments, totalCount) = await _commentRepository.GetPagedCommentsAsync(postId, pageNumber, pageSize);
            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var userAvatarsDic = await _mediaItemRepository.GetUrlDictionaryByRefIdsAsync(userIds);

            foreach (var comment in comments)
            {
                if (userAvatarsDic.TryGetValue(comment.UserId, out var avatarUrl))
                    comment.LogoUrl = avatarUrl;
            }

            return new PagedResultDto<CommentDto>(totalCount, comments);
        }

        public async Task<ResponseApi<CommentDto>> UpdateCommentAsync(Guid commentId, string newText)
        {
            var comment = await _commentRepository.GetAsync(commentId);
            await _postManager.UpdateCommentAsync(comment, newText, CurrentUser.GetId());
            var updatedComment = await _commentRepository.UpdateAsync(comment, autoSave: true);

            // استخدام _mapper بدلاً من ObjectMapper
            var dto = _mapper.Map<Comment, CommentDto>(updatedComment);
            dto.UserName = CurrentUser?.Name ?? string.Empty;
            dto.LogoUrl = await _mediaItemRepository.GetUrlByEntityIdAsync(CurrentUser.GetId());

            return new ResponseApi<CommentDto> { Data = dto, Success = true };
        }

        public async Task DeleteCommentAsync(Guid commentId)
        {
            var comment = await _commentRepository.GetAsync(commentId);
            await _commentRepository.DeleteAsync(comment);
        }

        #endregion

        #region Reactions

        public async Task ToggleReactionAsync(CreateUpdateReactionDto input)
        {
            var userId = CurrentUser.GetId();
            var existingReaction = await _reactionRepository.FirstOrDefaultAsync(r => r.PostId == input.PostId && r.UserId == userId);
            await _postManager.ToggleReactionAsync(existingReaction, input.PostId, userId, (ReactionType)input.Type);
        }

        #endregion
    }
}