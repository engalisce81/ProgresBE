using System;
using System.Collections.Generic;
namespace Dev.Acadmy.Courses
{
    public class CreateUpdateCourseDto
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; } = true; // enabled/disabled
        public bool IsLifetime { get; set; } = false;
        public int? DurationInDays { get; set; } // null if lifetime
        public bool IsPdf { get; set; }
        public string PdfUrl { get; set; }
        // هل يتم عرض عدد المشتركين للمستخدمين؟
        public bool ShowSubscriberCount { get; set; }
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }
        public Guid? SubjectId { get; set; }
        public bool IsQuiz { get; set; }
        public ICollection<string> Infos { get; set; }= new List<string>();
    }
}
