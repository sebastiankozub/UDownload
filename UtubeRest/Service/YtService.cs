using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using UtubeRest.Options;
using UtubeRest.ViewModel;

namespace UtubeRest.Service
{
    public class YtService : OsService
    {
        private readonly YtDlpOptions _ytDlpOptions;

        public YtService(IOptions<YtDlpOptions> ytDlpOptions)
        {
            _ytDlpOptions = ytDlpOptions.Value;
        }

        public string GetCookiesParameterPublic()
        {
            if (_ytDlpOptions.UseCookies && !string.IsNullOrEmpty(_ytDlpOptions.CookiesFilePath))
            {
                if (File.Exists(_ytDlpOptions.CookiesFilePath))
                {
                    return $"--cookies {_ytDlpOptions.CookiesFilePath}";
                }
                else
                {
                    Console.WriteLine($"Warning: Cookies file not found at {_ytDlpOptions.CookiesFilePath}");
                }
            }
            return string.Empty;
        }

        public string GetYtServiceVersion()
        {
            var ytDlpVersionCommand = "yt-dlp --version";

            var ytDlpVersion = RunUnixCommand(ytDlpVersionCommand);

            return ytDlpVersion;
        }

        public async Task<AvYtManifest> GetAvManifestAsync(string url)
        {
            var param = BuildCommonArgs();
            var ytDlpVideoManifestCommand = $"yt-dlp {url} {param} --dump-json".Trim();
            var ytDlpManifest = await RunUnixCommandAsync(ytDlpVideoManifestCommand);

            var ytManifestObject = JsonSerializer.Deserialize<AvYtManifest>(ytDlpManifest);

            return ytManifestObject;
        }

        public async Task<IEnumerable<AvYtFormatManifest>> GetAvFormatsAsync(string url)
        {
            var avManifest = await GetAvManifestAsync(url);

            if (avManifest.Formats != null && avManifest.Formats.Count() > 0)
            {
                return avManifest.Formats;
            }
            else
            {
                return [];
            }
        }

        private string BuildCommonArgs()
        {
            var sb = new StringBuilder();

            if (_ytDlpOptions.UseCookies && !string.IsNullOrEmpty(_ytDlpOptions.CookiesFilePath) && File.Exists(_ytDlpOptions.CookiesFilePath))
                sb.Append($" --cookies \"{_ytDlpOptions.CookiesFilePath}\"");

            if (!string.IsNullOrWhiteSpace(_ytDlpOptions.UserAgent))
                sb.Append($" --user-agent \"{_ytDlpOptions.UserAgent}\"");

            if (!string.IsNullOrWhiteSpace(_ytDlpOptions.RateLimit))
                sb.Append($" --rate-limit {_ytDlpOptions.RateLimit}");

            if (_ytDlpOptions.SleepRequestsSeconds > 0)
                sb.Append($" --sleep-requests {_ytDlpOptions.SleepRequestsSeconds} --max-sleep-interval {_ytDlpOptions.MaxSleepIntervalSeconds}");

            if (!string.IsNullOrWhiteSpace(_ytDlpOptions.ExtractorArgs))
                sb.Append($" --extractor-args \"{_ytDlpOptions.ExtractorArgs}\"");

            if (_ytDlpOptions.Retries > 0)
                sb.Append($" --retries {_ytDlpOptions.Retries}");

            if (_ytDlpOptions.FragmentRetries > 0)
                sb.Append($" --fragment-retries {_ytDlpOptions.FragmentRetries}");

            return sb.ToString();
        }
    }
}
