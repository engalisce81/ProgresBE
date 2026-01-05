using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Lectures
{
    public class CreateUpdateLectureDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }
        public Guid ChapterId { get; set; }
        public bool IsVisible { get; set; }
        public int QuizTime { get; set; }
        public int QuizTryCount { get; set; }
        public int QuizCount { get; set; }
        public bool IsFree { get; set; }
        public bool IsRequiredQuiz { get; set; }
        public int SuccessQuizRate { get; set; }
        public string? TelegramVideoUrl { get; set; }
        public ICollection<string> PdfUrls { get; set; } = new List<string>();

    }
}
