using System.Diagnostics;

namespace UtubeRest.Service
{
    public class FfmpegService : OsService
    {

        public async Task<string> GetFfmpegVersiopnAsync()
        {
            var ffMpegVersionCommmand = "ffmpeg -version";

            var ffMpegVersion = RunUnixCommand(ffMpegVersionCommmand);

            var ffMepegVersion = await RunUnixCommandAsync(ffMpegVersionCommmand);

            return ffMpegVersion;
        }


    }
}
