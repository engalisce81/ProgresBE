using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Dev.Acadmy.Entities.Advertisementes.Entities
{
    public class Advertisement : AuditedAggregateRoot<Guid>
    {
        public string Title { get; set; }

        // روابط الفيديوهات
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }

        // خصائص مساعدة (Read-only) للتأكد من وجود الرابط
        public bool HasYouTubeVideo => !string.IsNullOrWhiteSpace(YouTubeVideoUrl);
        public bool HasDriveVideo => !string.IsNullOrWhiteSpace(DriveVideoUrl);

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        protected Advertisement() { }

        public Advertisement(
            string title,
            string? youtubeUrl,
            string? driveUrl,
            DateTime startDate,
            DateTime endDate,
            bool isActive = true) 
        {
            Title = title;
            YouTubeVideoUrl = youtubeUrl;
            DriveVideoUrl = driveUrl;
            StartDate = startDate;
            EndDate = endDate;
            IsActive = isActive;
        }
    }
}