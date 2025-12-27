using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using UtubeRest.Service;

namespace UtubeRest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly YtService _ytService;

        public ValuesController(YtService ytService)
        {
            _ytService = ytService;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public async Task<AvYtManifest> Get()
        {
            var ffMpegVersionCommmand = "ffmpeg -version";

            var cookiesParam = _ytService.GetCookiesParameterPublic();

            var ytDlpDownloadCommand = $"yt-dlp -f 18 {cookiesParam} https://www.youtube.com/watch?v=1LUvA2GibSY".Trim();

            var ffmepegVersion = await YtService.RunUnixCommandAsync(ffMpegVersionCommmand);
            var ytDlpVersion = await YtService.RunUnixCommandAsync("yt-dlp --version");

            var avManifest = await _ytService.GetAvManifestAsync("https://www.youtube.com/watch?v=5_c_lL3G-Qo");

            using var outputStream = new MemoryStream();
            using var errorStream = new MemoryStream();

            using var outputStreamWriter = new StreamWriter(outputStream, Encoding.UTF8);
            using var errorStreamWriter = new StreamWriter(errorStream, Encoding.UTF8);

            var ytDlpVideoManifestCommand = $"yt-dlp {cookiesParam} https://www.youtube.com/watch?v=6n3pFFPSlW4 --dump-json".Trim();
            await YtService.RunUnixCommandAsync(ytDlpVideoManifestCommand, outputStreamWriter, errorStreamWriter);

            outputStream.Position = 0;
            errorStream.Position = 0;

            var ytManifestObject = await JsonSerializer.DeserializeAsync<AvYtManifest>(outputStream);

            return avManifest;
        }

        // POST api/values/download
        // Downloads:
        // - mode="merged" (default): best video+audio merged (mp4/mkv based on best formats)
        // - mode="separate": saves bestvideo and bestaudio as two files
        [HttpPost("download")]
        public async Task<IActionResult> DownloadByUrl([FromBody] DownloadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                return BadRequest("Url is required.");

            var cookiesParam = _ytService.GetCookiesParameterPublic();
            var downloadsDir = "/home/app/downloads";

            // Ensure downloads directory exists (safe if already present)
            await YtService.RunUnixCommandAsync($"mkdir -p {downloadsDir}");

            // Common args to be a bit nicer to YouTube while debugging
            // var common = $"{cookiesParam} --rate-limit 2M --sleep-requests 1 --max-sleep-interval 3 --retries 6 --fragment-retries 6";
            // Common args to be a bit nicer to YouTube while debugging
            var common = $"{cookiesParam} --rate-limit 2M --sleep-requests 1 --min-sleep-interval 1 --max-sleep-interval 3 --retries 6 --fragment-retries 6";
            string cmd;
            if (string.Equals(request.Mode, "separate", StringComparison.OrdinalIgnoreCase))
            {
                // Save best video only
                var videoOut = $"{downloadsDir}/%(title)s.%(id)s.video.%(ext)s";
                var audioOut = $"{downloadsDir}/%(title)s.%(id)s.audio.%(ext)s";

                var videoCmd = $"yt-dlp {common} -f bestvideo -o \"{videoOut}\" \"{request.Url}\"";
                var audioCmd = $"yt-dlp {common} -f bestaudio -o \"{audioOut}\" \"{request.Url}\"";

                var videoOutLog = await YtService.RunUnixCommandAsync(videoCmd);
                var audioOutLog = await YtService.RunUnixCommandAsync(audioCmd);

                return Ok(new
                {
                    mode = "separate",
                    url = request.Url,
                    videoCommand = videoCmd,
                    audioCommand = audioCmd,
                    downloadsDir
                });
            }
            else
            {
                // Merge best video+audio into a single file (requires ffmpeg in image, already installed)
                var mergedOut = "/home/app/downloads/%(title)s.%(id)s.%(ext)s";

                // Let yt-dlp pick bestvideo+bestaudio and merge via ffmpeg
                cmd = $"yt-dlp {common}  -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best\" -o '{mergedOut}' \"{request.Url}\"";

                var outLog = await YtService.RunUnixCommandAsync(cmd);

                // Save best video only
                var videoOut = "/home/app/downloads/%(title)s.%(id)s.video.%(ext)s";
                var audioOut = "/home/app/downloads/%(title)s.%(id)s.audio.%(ext)s";
                var videoCmd = $"yt-dlp {common} -f bestvideo -o '{videoOut}' \"{request.Url}\"";
                var audioCmd = $"yt-dlp {common} -f bestaudio -o '{audioOut}' \"{request.Url}\"";

                var videoOutLog = await YtService.RunUnixCommandAsync(videoCmd);
                var audioOutLog = await YtService.RunUnixCommandAsync(audioCmd);

                return Ok(new
                {
                    mode = "merged",
                    url = request.Url,
                    command = cmd,
                    downloadsDir
                });
            }
        }

        public class DownloadRequest
        {
            public string Url { get; set; } = string.Empty;
            // "merged" or "separate"
            public string Mode { get; set; } = "merged";
        }

        // existing endpoints...
        [HttpGet("{id}")]
        public string Get(int id) => "value";

        [HttpPost]
        public void Post([FromBody] string value) { }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value) { }

        [HttpDelete("{id}")]
        public void Delete(int id) { }
    }
}
