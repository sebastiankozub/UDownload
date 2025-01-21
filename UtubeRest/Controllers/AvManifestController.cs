using Microsoft.AspNetCore.Mvc;
using UtubeRest.ViewModel;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UtubeRest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvManifestController : ControllerBase
    {
        // GET: api/<AvManifestController>
        [HttpGet]
        public async Task<IEnumerable<AvManifest>> Get(params string[] ids)
        {
            throw new NotImplementedException(nameof(AvManifestController));

            //var youtube = new YoutubeClient();
            //var videoId = YoutubeExplode.Videos.VideoId.Parse(ids[0]);

            //var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            //var videoStreamInfos = manifest.GetVideoOnlyStreams();
            //var audioStreamInfos = manifest.GetAudioOnlyStreams();

            //return new string[] { "value1", "value2" };
        }

        // GET api/<AvManifestController>/5
        [HttpGet("{avResourceId}")]
        public async Task<AvManifest> Get(string avResourceId)
        {
            var youtube = new YoutubeClient();

            var videoId = YoutubeExplode.Videos.VideoId.Parse(avResourceId);  // try to not throw internal exception

            var video = await youtube.Videos.GetAsync(videoId);
            //var streams = youtube.Videos.Streams;

            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

            var videoStreamInfos = manifest.GetVideoStreams();
            var audioStreamInfos = manifest.GetAudioStreams();

            var videoOnlyStreamInfos = manifest.GetVideoOnlyStreams();
            var audioOnlyStreamInfos = manifest.GetAudioOnlyStreams();

            var bestQuStream = videoOnlyStreamInfos.TryGetWithHighestVideoQuality();

            if (bestQuStream is null) ; // throw new Exception("No video stream found");

            return new AvManifest() {
                Id = videoId,
                Title = video.Title,
                Description = video.Description,
                Keywords = video.Keywords,
                UploadDate = video.UploadDate,
                AudioStreams = audioStreamInfos.Select(x => new AudioStream()
                {
                    Url = x.Url,
                    Container = x.Container.ToString(),
                    Size = x.Size.ToString(),
                    Bitrate = x.Bitrate.ToString(),
                    AudioCodec = x.AudioCodec.ToString(),
                    AudioLanguage =x.AudioLanguage.ToString(),
                    IsAudioLanguageDefault = x.IsAudioLanguageDefault.ToString(),
                    HashId = GetSha256HashFrom(x.Url),
                }).ToList(),
                VideoStreams = videoStreamInfos.Select(x => new VideoStream()
                {
                    Url = x.Url,
                    Container = x.Container.ToString(),
                    Size = x.Size.ToString(),
                    Bitrate = x.Bitrate.ToString(),
                    VideoCodec = x.VideoCodec.ToString(),
                    VideoQuality = x.VideoQuality.ToString(),
                    VideoResolution = x.VideoResolution.ToString(),
                    HashId = GetSha256HashFrom(x.Url),
                }).ToList(),           
            };            
        }

        public static string GetSha256HashFrom(string text) =>
            BitConverter.ToString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty);

        //// POST api/<AvManifestController>
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/<AvManifestController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<AvManifestController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
