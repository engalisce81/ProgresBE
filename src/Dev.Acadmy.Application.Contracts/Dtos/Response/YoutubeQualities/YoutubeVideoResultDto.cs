using System.Collections.Generic;

namespace Dev.Acadmy.Dtos.Response.YoutubeQualities
{
    public class YoutubeVideoResultDto 
    {
        public string Title { get; set; }
        public List<YoutubeQualityDto> Qualities { get; set; } = new List<YoutubeQualityDto>();
    }
}
