using System.Text.Json;
using UtubeRest.ViewModel;

namespace UtubeRest.Service
{
    public class YtService : OsService
    {

        public string GetYtServiceVersion()
        {
            var ytDlpVersionCommand = "yt-dlp --version";

            var ytDlpVersion = RunUnixCommand(ytDlpVersionCommand);

            return ytDlpVersion;
        }

        public async Task<AvYtManifest> GetAvManifestAsync(string url)
        {
            var ytDlpVideoManifestCommand = $"yt-dlp {url} --dump-json";
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

            //using (JsonDocument document = JsonDocument.Parse(avManifest))
            //{
            //    JsonElement root = document.RootElement;
            //    JsonElement formats = root.GetProperty("formats");
            //    foreach (JsonElement format in formats.EnumerateArray())
            //    {
            //        if (format.TryGetProperty("url", out JsonElement formatElement))
            //        {
            //            yield return new AudioAvStream()
            //            {
            //                AudioCodec = "acodec",
            //                AudioLanguage = "language",
            //                Bitrate = "abr",
            //                Container = "ext",
            //                HashId = "format_id",
            //                IsAudioLanguageDefault = "language",
            //                Size = "filesize",
            //                Url = formatElement.GetRawText()
            //            };
            //        }
            //    }
            //}
        }
    }
}
