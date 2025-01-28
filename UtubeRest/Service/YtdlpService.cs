using System.Text.Json;
using UtubeRest.ViewModel;

namespace UtubeRest.Service
{
    public class YtdlpService : OsService
    {

        public string GetYtDlpVersiopn()
        {
            var ytDlpVersionCommand = "yt-dlp --version";

            var ytDlpVersion = RunUnixCommand(ytDlpVersionCommand);

            return ytDlpVersion;
        }

        public string GetAvManifest(string url)
        {
            var ytDlpVideoManifestCommand = $"yt-dlp {url} --dump-json";
            var ytDlpManifest = RunUnixCommand(ytDlpVideoManifestCommand);
            return ytDlpManifest;
        }

        public IEnumerable<AvStream> GetAvFormats(string url)
        {
            var avManifest = GetAvManifest(url);

            using (JsonDocument document = JsonDocument.Parse(avManifest))
            {
                JsonElement root = document.RootElement;
                JsonElement formats = root.GetProperty("formats");
                foreach (JsonElement format in formats.EnumerateArray())
                {
                    if (format.TryGetProperty("url", out JsonElement formatElement))
                    {
                        yield return new AudioAvStream()
                        {
                            AudioCodec = "acodec",
                            AudioLanguage = "language",
                            Bitrate = "abr",
                            Container = "ext",
                            HashId = "format_id",
                            IsAudioLanguageDefault = "language",
                            Size = "filesize",
                            Url = formatElement.GetRawText()
                        };              
                    }
                }
            }

        }
    }
}
