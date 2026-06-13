using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UtubeRest.Options;
using UtubeRest.ViewModel;

namespace UtubeRest.Service
{
    public class YtService : OsService
    {
        private static readonly JsonSerializerOptions SearchJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly YtDlpOptions _ytDlpOptions;
        private readonly ILogger<YtService> _logger;

        public YtService(IOptions<YtDlpOptions> ytDlpOptions, ILogger<YtService> logger)
        {
            _ytDlpOptions = ytDlpOptions.Value;
            _logger = logger;
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
            var ytDlpVideoManifestCommand = BuildManifestCommand(url);
            var (ytDlpManifest, errorOutput) = await RunCommandCaptureAsync(ytDlpVideoManifestCommand);

            if (string.IsNullOrWhiteSpace(ytDlpManifest))
            {
                throw new InvalidOperationException($"yt-dlp returned no manifest output. {errorOutput}".Trim());
            }

            var ytManifestObject = JsonSerializer.Deserialize<AvYtManifest>(ytDlpManifest, SearchJsonOptions);
            if (ytManifestObject is null)
            {
                throw new InvalidOperationException("yt-dlp manifest output could not be deserialized.");
            }

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

        public async Task<IEnumerable<SearchResult>> SearchAsync(string query, int count)
        {
            var results = new List<SearchResult>();

            await SearchStreamAsync(
                query,
                count,
                result =>
                {
                    results.Add(result);
                    return Task.CompletedTask;
                });

            return results;
        }

        public async Task SearchStreamAsync(
            string query,
            int count,
            Func<SearchResult, Task> onResult,
            CancellationToken cancellationToken = default)
        {
            var cmd = BuildSearchCommand(query, count);

            await RunUnixCommandStreamingAsync(
                cmd,
                async line =>
                {
                    var result = ParseSearchResult(line);
                    if (result is not null)
                    {
                        await onResult(result);
                    }
                },
                cancellationToken: cancellationToken);
        }

        public async Task<AvManifest> FetchManifestAsync(string videoIdOrUrl)
        {
            var url = NormalizeToYoutubeUrl(videoIdOrUrl);
            var manifest = await GetAvManifestAsync(url);
            var availableFormatIds = await GetAvailableFormatIdsAsync(url);

            if (availableFormatIds.Count > 0)
            {
                manifest.Formats = (manifest.Formats ?? [])
                    .Where(format => availableFormatIds.Contains(format.FormatId))
                    .ToList();
            }

            return MapManifest(manifest);
        }

        public async Task DownloadSelectedFormatsAsync(
            string videoIdOrUrl,
            string videoFormatId,
            string audioFormatId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(videoFormatId))
            {
                throw new ArgumentException("Video format ID is required.", nameof(videoFormatId));
            }

            if (string.IsNullOrWhiteSpace(audioFormatId))
            {
                throw new ArgumentException("Audio format ID is required.", nameof(audioFormatId));
            }

            var url = NormalizeToYoutubeUrl(videoIdOrUrl);
            const string outputTemplate = "/home/app/downloads/%(title)s.%(id)s.%(ext)s";
            var formatSelector = $"{videoFormatId}+{audioFormatId}";

            var command = $"yt-dlp {BuildCommonArgs()} -f {ShellQuote(formatSelector)} -o {ShellQuote(outputTemplate)} {ShellQuote(url)}".Trim();
            var result = await RunUnixCommandWithResultAsync(command, cancellationToken);

            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"yt-dlp download failed for {url}. {result.Error}".Trim());
            }

            if (!string.IsNullOrWhiteSpace(result.Output))
            {
                _logger.LogInformation(
                    "yt-dlp completed for {VideoIdOrUrl} with video format {VideoFormatId} and audio format {AudioFormatId}:{NewLine}{Output}",
                    videoIdOrUrl,
                    videoFormatId,
                    audioFormatId,
                    Environment.NewLine,
                    result.Output);
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
                sb.Append($" --sleep-requests {_ytDlpOptions.SleepRequestsSeconds}");
                //sb.Append($" --sleep-requests {_ytDlpOptions.SleepRequestsSeconds} --max-sleep-interval {_ytDlpOptions.MaxSleepIntervalSeconds}");

            if (!string.IsNullOrWhiteSpace(_ytDlpOptions.ExtractorArgs))
                sb.Append($" --extractor-args {ShellQuote(_ytDlpOptions.ExtractorArgs)}");

            if (_ytDlpOptions.Retries > 0)
                sb.Append($" --retries {_ytDlpOptions.Retries}");

            if (_ytDlpOptions.FragmentRetries > 0)
                sb.Append($" --fragment-retries {_ytDlpOptions.FragmentRetries}");

            return sb.ToString();
        }


        private string BuildCommonArgsSimple()
        {
            var sb = new StringBuilder();

            if (_ytDlpOptions.UseCookies && !string.IsNullOrEmpty(_ytDlpOptions.CookiesFilePath) && File.Exists(_ytDlpOptions.CookiesFilePath))
                sb.Append($" --cookies {ShellQuote(_ytDlpOptions.CookiesFilePath)}");

            //if (!string.IsNullOrWhiteSpace(_ytDlpOptions.UserAgent))
            //    sb.Append($" --user-agent \"{_ytDlpOptions.UserAgent}\"");

            return sb.ToString();
        }

        private string BuildSearchCommand(string query, int count)
        {
            var param = BuildCommonArgsSimple();
            var jsRuntime = "--js-runtime node";
            var searchArg = ShellQuote($"ytsearch{count}:{query}");

            return $"yt-dlp {jsRuntime} {param} --dump-json {searchArg}".Trim();   // --dump-json --flat-playlist  instead of --print-json
        }

        private string BuildManifestCommand(string url)
        {
            var param = BuildManifestArgs();
            var jsRuntime = "--js-runtime node";
            //--dump-json, and add --flat-playlist
            //return $"yt-dlp {jsRuntime} {param} --no-progress --no-warnings --no-playlist --print-json {ShellQuote(url)}".Trim();
            return $"yt-dlp {jsRuntime} {param} --no-progress --no-warnings --no-playlist --dump-json {ShellQuote(url)}".Trim();
        }

        private string BuildListFormatsCommand(string url)
        {
            var param = BuildManifestArgs();
            return $"yt-dlp {param} --no-progress --no-warnings --no-playlist -F {ShellQuote(url)}".Trim();
        }

        private string BuildManifestArgs()
        {
            var sb = new StringBuilder();

            if (_ytDlpOptions.UseCookies && !string.IsNullOrEmpty(_ytDlpOptions.CookiesFilePath) && File.Exists(_ytDlpOptions.CookiesFilePath))
                sb.Append($" --cookies {ShellQuote(_ytDlpOptions.CookiesFilePath)}");

            if (!string.IsNullOrWhiteSpace(_ytDlpOptions.UserAgent))
                sb.Append($" --user-agent {ShellQuote(_ytDlpOptions.UserAgent)}");

            if (!string.IsNullOrWhiteSpace(_ytDlpOptions.ExtractorArgs))
                sb.Append($" --extractor-args {ShellQuote(_ytDlpOptions.ExtractorArgs)}");

            return sb.ToString();
        }

        private async Task<(string Output, string Error)> RunCommandCaptureAsync(string command, CancellationToken cancellationToken = default)
        {
            var result = await RunUnixCommandWithResultAsync(command, cancellationToken);
            return (result.Output, result.Error);
        }

        private async Task<HashSet<string>> GetAvailableFormatIdsAsync(string url, CancellationToken cancellationToken = default)
        {
            var command = BuildListFormatsCommand(url);
            var (output, error) = await RunCommandCaptureAsync(command, cancellationToken);
            var combined = string.Join(
                Environment.NewLine,
                new[] { output, error }.Where(static text => !string.IsNullOrWhiteSpace(text)));

            if (string.IsNullOrWhiteSpace(combined))
            {
                return [];
            }

            return combined
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseFormatIdFromListLine)
                .Where(static formatId => !string.IsNullOrWhiteSpace(formatId))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static string? ParseFormatIdFromListLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)
                || line.StartsWith("[", StringComparison.Ordinal)
                || line.StartsWith("-", StringComparison.Ordinal))
            {
                return null;
            }

            var match = Regex.Match(line, @"^(?<id>\S+)\s+\S+");
            if (!match.Success)
            {
                return null;
            }

            var formatId = match.Groups["id"].Value;
            return formatId.All(char.IsLetterOrDigit) ? formatId : null;
        }

        private static SearchResult? ParseSearchResult(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            try
            {
                var result = JsonSerializer.Deserialize<SearchResult>(line, SearchJsonOptions);
                if (result is null || string.IsNullOrWhiteSpace(result.Id) || string.IsNullOrWhiteSpace(result.Title))
                {
                    return null;
                }

                result.WebpageUrl ??= $"https://www.youtube.com/watch?v={result.Id}";
                return result;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private AvManifest MapManifest(AvYtManifest manifest)
        {
            return new AvManifest
            {
                Id = manifest.Id,
                Title = manifest.Title,
                Channel = manifest.Channel,
                Uploader = manifest.Uploader,
                Thumbnail = manifest.Thumbnail,
                ViewCount = manifest.ViewCount,
                Duration = manifest.Duration,
                UploadDate = ParseUploadDate(manifest.UploadDateRaw),
                Description = manifest.Description ?? string.Empty,
                Keywords = manifest.Tags ?? [],
                AudioStreams = (manifest.Formats ?? [])
                    .Where(IsAudioOnlyFormat)
                    .Select(MapAudioStream)
                    .ToList(),
                VideoStreams = (manifest.Formats ?? [])
                    .Where(IsVideoOnlyFormat)
                    .Select(MapVideoStream)
                    .ToList(),
            };
        }

        private static bool IsAudioOnlyFormat(AvYtFormatManifest format)
        {
            return HasDownloadUrl(format) && !IsNone(format.Acodec) && IsNone(format.Vcodec);
        }

        private static bool IsVideoOnlyFormat(AvYtFormatManifest format)
        {
            return HasDownloadUrl(format) && !IsNone(format.Vcodec) && IsNone(format.Acodec);
        }

        private static bool HasDownloadUrl(AvYtFormatManifest format)
        {
            return !string.IsNullOrWhiteSpace(format.Url);
        }

        private static bool IsNone(string? codec)
        {
            return string.IsNullOrWhiteSpace(codec) || string.Equals(codec, "none", StringComparison.OrdinalIgnoreCase);
        }

        private static AudioAvStream MapAudioStream(AvYtFormatManifest format)
        {
            return new AudioAvStream
            {
                HashId = ComputeHash(format.Url ?? format.FormatId),
                FormatId = format.FormatId,
                FormatNote = format.FormatNote,
                Url = format.Url ?? string.Empty,
                Container = format.Container ?? format.Ext ?? string.Empty,
                Size = FormatFileSize(format.Filesize) ?? FormatFileSize(format.FilesizeApprox) ?? string.Empty,
                Bitrate = FormatBitrate(format.Abr, format.Tbr),
                AudioCodec = format.Acodec ?? string.Empty,
                AudioLanguage = format.Language,
                IsAudioLanguageDefault = (format.LanguagePreference == 10).ToString(),
            };
        }

        private static VideoAvStream MapVideoStream(AvYtFormatManifest format)
        {
            return new VideoAvStream
            {
                HashId = ComputeHash(format.Url ?? format.FormatId),
                FormatId = format.FormatId,
                FormatNote = format.FormatNote,
                Url = format.Url ?? string.Empty,
                Container = format.Container ?? format.Ext ?? string.Empty,
                Size = FormatFileSize(format.Filesize) ?? FormatFileSize(format.FilesizeApprox) ?? string.Empty,
                Bitrate = FormatBitrate(format.Vbr, format.Tbr),
                VideoCodec = format.Vcodec ?? string.Empty,
                VideoQuality = format.FormatNote ?? format.FormatDescription ?? string.Empty,
                VideoResolution = format.Resolution ?? BuildResolution(format.Width, format.Height),
            };
        }

        private static string ComputeHash(string input)
        {
            return BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).Replace("-", string.Empty);
        }

        private static string? FormatFileSize(object? value)
        {
            if (value is null)
            {
                return null;
            }

            if (value is long longValue)
            {
                return FormatFileSize(longValue);
            }

            if (value is int intValue)
            {
                return FormatFileSize(intValue);
            }

            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt64(out var elementLong))
                {
                    return FormatFileSize(elementLong);
                }
            }

            if (long.TryParse(value.ToString(), out var parsed))
            {
                return FormatFileSize(parsed);
            }

            return null;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] units = ["B", "KB", "MB", "GB", "TB"];
            double size = bytes;
            var unitIndex = 0;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.#} {units[unitIndex]}";
        }

        private static string FormatBitrate(float? primary, object? fallback)
        {
            if (primary.HasValue && primary.Value > 0)
            {
                return $"{primary.Value:0.#} kbps";
            }

            if (fallback is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetDouble(out var doubleValue))
            {
                return $"{doubleValue:0.#} kbps";
            }

            if (fallback is double fallbackDouble && fallbackDouble > 0)
            {
                return $"{fallbackDouble:0.#} kbps";
            }

            if (fallback is float fallbackFloat && fallbackFloat > 0)
            {
                return $"{fallbackFloat:0.#} kbps";
            }

            if (fallback is string fallbackString && double.TryParse(fallbackString, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return $"{parsed:0.#} kbps";
            }

            return string.Empty;
        }

        private static string BuildResolution(int? width, int? height)
        {
            if (width.HasValue && height.HasValue)
            {
                return $"{width}x{height}";
            }

            return string.Empty;
        }

        private static string NormalizeToYoutubeUrl(string videoIdOrUrl)
        {
            if (videoIdOrUrl.Contains("youtube.com", StringComparison.OrdinalIgnoreCase)
                || videoIdOrUrl.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                return videoIdOrUrl;
            }

            return $"https://www.youtube.com/watch?v={videoIdOrUrl}";
        }

        private static DateTimeOffset ParseUploadDate(string? rawUploadDate)
        {
            if (!string.IsNullOrWhiteSpace(rawUploadDate)
                && DateTime.TryParseExact(rawUploadDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
            {
                return new DateTimeOffset(parsedDate, TimeSpan.Zero);
            }

            return DateTimeOffset.MinValue;
        }

        private static string ShellQuote(string value)
        {
            return $"'{value.Replace("'", "'\"'\"'")}'";
        }
    }
}
