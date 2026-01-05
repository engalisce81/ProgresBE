using Dev.Acadmy.Quizzes;
using System;
using System.Collections.Generic;
namespace Dev.Acadmy.Lectures
{
    public class LectureInfoDto
    {
        public Guid LectureId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool HasYouTubeVideo { get; set; }
        public bool HasDriveVideo { get; set; }
        public bool HasTelegramVideo { get; set; }
        public string? TelegramVideoUrl { get; set; }
        public bool IsQuizRequired { get; set; }
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }
        public ICollection<string> PdfUrls { get; set; }  = new List<string>();  
        public QuizInfoDto Quiz { get; set; }
    }
}
