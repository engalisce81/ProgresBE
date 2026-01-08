using Dev.Acadmy.Chapters;
using Dev.Acadmy.Dtos.Response.Courses;
using System;
using System.Collections.Generic;

namespace Dev.Acadmy.Courses
{
    public class CourseInfoHomeDto
    {
        public Guid Id { get; set; }
        public bool IsPdf { get; set; }
        public string PdfUrl { get; set; }
        public bool IsQuiz { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string LogoUrl { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string TeacherLogoUrl { get; set; }
        public double Rating { get; set; } // average rating
        public Guid? CollegeId { get; set; }
        public string CollegeName { get; set; }
        public Guid? SubjectId { get; set; }
        // هل يتم عرض عدد المشتركين للمستخدمين؟
        public bool ShowSubscriberCount { get; set; } 
        public int SubscriberCount { get; set; }    
        public string? SubjectName { get; set; }
        public Guid? GradelevelId { get; set; }
        public string? GradelevelName { get; set; }
        public bool AlreadyJoin { get; set; }
        public bool AlreadyRequest { get; set;}
        public int ChapterCount { get; set; }
        public int LectureCount { get; set; }
        public bool HasYouTubeVideo { get; set; }
        public bool HasDriveVideo { get; set; }
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }
        public int? DurationInWeeks { get; set; } // null if lifetime
        public ICollection<string> Infos { get;set;} = new List<string>();
        public ICollection<CourseChaptersDto> courseChaptersDtos { get; set; } = new List<CourseChaptersDto>();
        public ICollection<FeedbackDto> Feedbacks { get; set; } = new List<FeedbackDto>();


    }
}
