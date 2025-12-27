using Dev.Acadmy.Entities.Posts.Entities;
using Dev.Acadmy.Enums;
using Dev.Acadmy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Dev.Acadmy.Entities.Posts.Managers
{
    public class PostManager :DomainService
    {
        private readonly IPostRepository _postRepository;
        private readonly IReactionRepository _reactionRepository;
        public PostManager(IPostRepository postRepository , IReactionRepository reactionRepository)
        {
            _postRepository = postRepository;
            _reactionRepository = reactionRepository;
        }
        public async Task<Post> CreateAsync(string title, string content, bool isGeneral, Guid userId)
        {
            // قاعدة عمل: منع تكرار العناوين لنفس المستخدم في نفس اليوم
            var isDuplicate = await _postRepository.AnyAsync(p =>p.Title == title && p.UserId == userId && p.CreationTime.Date == DateTime.Now.Date);
            if (isDuplicate)throw new UserFriendlyException("لقد نشرت موضوعاً بنفس العنوان اليوم!");
            return new Post
            {
                Title = title,
                Content = content,
                IsGeneral = isGeneral,
                UserId = userId
            };
        }

        public async Task UpdateAsync(Post post, string title, string content, bool isGeneral, Guid currentUserId)
        {
            // التأكد أن المستخدم الذي يحاول التعديل هو صاحب المنشور
            if (post.UserId != currentUserId)
            {
                throw new UserFriendlyException("عذراً، لا يمكنك تعديل منشور لا تملكه!");
            }

            // تحديث الحقول
            post.Title = title;
            post.Content = content;
            post.IsGeneral = isGeneral;

            await Task.CompletedTask;
        }

        public async Task ToggleReactionAsync(
    Reaction existingReaction,
    Guid postId,
    Guid userId,
    ReactionType newType)
        {
            if (existingReaction != null)
            {
                // إذا كان المستخدم ضغط على نفس نوع التفاعل (مثلاً Like مرتين) -> يتم حذفه
                if (existingReaction.Type == newType)
                {
                    await _reactionRepository.DeleteAsync(existingReaction);
                }
                else
                {
                    // إذا كان نوع مختلف (كان Like وأصبح Love) -> يتم التحديث
                    existingReaction.Type = newType;
                    await _reactionRepository.UpdateAsync(existingReaction);
                }
            }
            else
            {
                // إذا لم يكن هناك تفاعل سابق -> إضافة تفاعل جديد
                var reaction = new Reaction
                {
                    PostId = postId,
                    UserId = userId,
                    Type = newType
                };
                await _reactionRepository.InsertAsync(reaction);
            }
        }

        public void CheckPolicy(Post post, Guid userId)
        {
            if (post.UserId != userId)
                throw new UserFriendlyException("لا تملك صلاحية الوصول لهذا المنشور");
        }

        public async Task UpdateCommentAsync(Comment comment, string newText, Guid currentUserId)
        {
            // التأكد من الملكية
            if (comment.UserId != currentUserId)
            {
                throw new UserFriendlyException("عذراً، لا يمكنك تعديل تعليق لا تملكه!");
            }

            // التحقق من صحة البيانات (Validation)
            if (string.IsNullOrWhiteSpace(newText))
            {
                throw new UserFriendlyException("لا يمكن أن يكون التعليق فارغاً");
            }

            // تحديث النص
            comment.Text = newText;

            await Task.CompletedTask;
        }
    }
}
