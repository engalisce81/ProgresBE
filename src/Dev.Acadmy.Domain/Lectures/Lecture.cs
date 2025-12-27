
using Dev.Acadmy.Quizzes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Lectures
{
    public class Lecture : AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }
        public bool HasYouTubeVideo => !string.IsNullOrWhiteSpace(YouTubeVideoUrl);
        public bool HasDriveVideo => !string.IsNullOrWhiteSpace(DriveVideoUrl);
        public Guid ChapterId { get; set; }
        public bool IsFree { get; set; }
        public bool IsVisible { get; set; }
        public bool IsRequiredQuiz { get; set; }
        public int QuizTryCount { get; set; }
        public int SuccessQuizRate { get; set; }

        [ForeignKey(nameof(ChapterId))]
        public Chapters.Chapter Chapter { get; set; }
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();  
    }
}
