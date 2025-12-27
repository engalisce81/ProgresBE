using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Acadmy.Dtos.Request.Advertisementes
{
    public class CreateUpdateAdvertisementDto
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string YouTubeVideoUrl { get; set; }
        public string DriveVideoUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
