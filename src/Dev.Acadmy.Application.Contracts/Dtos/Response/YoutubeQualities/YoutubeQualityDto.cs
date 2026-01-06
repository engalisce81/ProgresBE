namespace Dev.Acadmy.Dtos.Response.YoutubeQualities
{
    public class YoutubeQualityDto
    {
        public string Label { get; set; }
        public int Resolution { get; set; }
        public string VideoUrl { get; set; } // رابط الفيديو
        public string? AudioUrl { get; set; }  // رابط الصوت المنفصل
        public bool IsAdaptive { get; set; } // هل يحتاج دمج في الموبايل؟
    }
}
