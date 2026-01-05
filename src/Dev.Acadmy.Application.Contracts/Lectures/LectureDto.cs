using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Lectures
{
    public class LectureDto:EntityDto<Guid>
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool HasYouTubeVideo { get; set; }
        public bool HasDriveVideo { get; set; }
        public bool HasTelegramVideo { get; set; }
        public string? TelegramVideoUrl { get; set; }

        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }
        
        public Guid ChapterId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string ChapterName { get; set; }
        public int QuizTime { get; set; }
        public int QuizTryCount { get; set; }
        public int QuizCount {  get; set; }
        public bool IsVisible { get; set; }
        public bool IsFree { get; set; }
        public bool IsRequiredQuiz { get; set; }
        public int SuccessQuizRate { get; set; }
        public ICollection<string> PdfUrls { get; set; } = new List<string>();

    }
}
