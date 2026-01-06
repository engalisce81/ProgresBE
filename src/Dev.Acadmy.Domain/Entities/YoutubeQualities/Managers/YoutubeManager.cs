using Dev.Acadmy.Entities.YoutubeQualities.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Dev.Acadmy.Entities.YoutubeQualities.Managers
{
    public class YoutubeManager : DomainService
    {
        public async Task<Dictionary<string, YoutubeVideoResult>> GetQualitiesDictAsync(List<string> videoUrls)
        {
            var _dict = new Dictionary<string, YoutubeVideoResult>();
            var _youtubeClient = new YoutubeClient();
            var _baseUrl = "https://localhost:44318";
            var _uniqueUrls = videoUrls.Where(u => !string.IsNullOrEmpty(u)).Distinct().ToList();

            foreach (var _url in _uniqueUrls)
            {
                try
                {
                    var _urlWithLocale = _url.Contains("?") ? $"{_url}&hl=ar" : $"{_url}?hl=ar";
                    var _video = await _youtubeClient.Videos.GetAsync(_urlWithLocale);
                    var _streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(_video.Id);

                    var _audioStreams = _streamManifest.GetAudioOnlyStreams();
                    var _finalAudio = _audioStreams.GetWithHighestBitrate();

                    // الجودات العالية: سنقوم بدمجها عبر رابط Proxy داخلي
                    var _highResStreams = _streamManifest.GetVideoOnlyStreams()
                        .Where(s => s.VideoQuality.MaxHeight >= 720)
                        .Select(_s => new YoutubeQuality
                        {
                            Label = _s.VideoQuality.Label,
                            Resolution = _s.VideoQuality.MaxHeight,
                            // الرابط سيوجه المستخدم لـ Controller الدمج في سيرفرك
                            VideoUrl = $"{_baseUrl}/api/youtube/stream?vUrl={Uri.EscapeDataString(_s.Url)}&aUrl={Uri.EscapeDataString(_finalAudio.Url)}",
                            AudioUrl = null,
                            IsAdaptive = false // أصبح مدمجاً الآن بالنسبة للموبايل
                        }).ToList();

                    // الجودات المدمجة أصلاً (مثل 360p) تبقى كما هي
                    var _muxedStreams = _streamManifest.GetMuxedStreams()
                        .Select(_s => new YoutubeQuality
                        {
                            Label = _s.VideoQuality.Label,
                            Resolution = _s.VideoQuality.MaxHeight,
                            VideoUrl = _s.Url,
                            AudioUrl = null,
                            IsAdaptive = false
                        }).ToList();

                    _dict[_url] = new YoutubeVideoResult
                    {
                        Title = _video.Title,
                        Qualities = _highResStreams.Concat(_muxedStreams).OrderByDescending(x => x.Resolution).ToList()
                    };
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Youtube Error: {ex.Message}");
                    _dict[_url] = null;
                }
            }
            return _dict;
        }
    }
}
