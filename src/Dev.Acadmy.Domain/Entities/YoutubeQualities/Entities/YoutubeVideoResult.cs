using System.Collections.Generic;

namespace Dev.Acadmy.Entities.YoutubeQualities.Entities
{
    public class YoutubeVideoResult
    {
        public string Title { get; set; }
        public List<YoutubeQuality> Qualities { get; set; }
    }
}
