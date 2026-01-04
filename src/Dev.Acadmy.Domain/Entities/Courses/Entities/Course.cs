using Dev.Acadmy.Chapters;
using Dev.Acadmy.Courses;
using Dev.Acadmy.Exams;
using Dev.Acadmy.Questions;
using Dev.Acadmy.Quizzes;
using Dev.Acadmy.Universites;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Dev.Acadmy.Entities.Courses.Entities
{
    public class Course : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
        public string Title { get; set;}
        public string Description { get; set;}
        public decimal Price { get; set;}
        public float Rating { get; set; } // average rating
        public Guid UserId { get; set; }
        public Guid? CollegeId { get; set;}
        public Guid? SubjectId { get; set; }

        // Visibility
        public bool IsActive { get; set; } = true; // enabled/disabled
        public bool IsQuiz { get;set; } = false;

        // Purchase & duration
        public bool IsLifetime { get; set; } = false;
        public bool IsPdf { get; set; } 
        public string PdfUrl {  get; set; }
        // هل يتم عرض عدد المشتركين للمستخدمين؟
        public bool ShowSubscriberCount { get; set; } = true;
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }

        // خصائص مساعدة (Read-only) للتأكد من وجود الرابط
        public bool HasYouTubeVideo => !string.IsNullOrWhiteSpace(YouTubeVideoUrl);
        public bool HasDriveVideo => !string.IsNullOrWhiteSpace(DriveVideoUrl);
        public int? DurationInDays { get; set; } // null if lifetime
        [ForeignKey(nameof(CollegeId))]
        public College College { get; set; }
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
        [ForeignKey(nameof(SubjectId))]
        public Subject? Subject { get; set; }
        public CourseCertificate? CourseCertificate { get; set; }
        public ICollection<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<Chapter>  Chapters { get; set; } = new List<Chapter>();
        public ICollection<CourseInfo> CourseInfos { get; set; } = new List<CourseInfo>();
        public ICollection<CourseFeedback> Feedbacks { get; set; } = new List<CourseFeedback>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
