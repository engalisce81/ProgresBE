using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Dev.Acadmy.Dtos.Response.Advertisementes
{
    public class AdvertisementDto:EntityDto<Guid>
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string? YouTubeVideoUrl { get; set; }
        public string? DriveVideoUrl { get; set; }

        // خصائص مساعدة (Read-only) للتأكد من وجود الرابط
        public bool HasYouTubeVideo { get; set; }
        public bool HasDriveVideo { get; set; }
            public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
