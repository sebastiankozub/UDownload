using Humanizer.Bytes;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace UtubeRest.ViewModel
{
    public class AvManifest
    {
        //public required string Title { get; set; }

        //public VideoId Id { get; }
        public required string Id { get; set; }

        public string Url => $"https://www.youtube.com/watch?v={Id}";

        public required string Title { get; set; }

        //public Author Author { get; } = author;

        public DateTimeOffset UploadDate { get; set; }

        public required string Description { get; set; }

        //public TimeSpan? Duration { get; set; }

        //public IReadOnlyList<Thumbnail> Thumbnails { get; } = thumbnails;

        public required IReadOnlyList<string> Keywords { get; set; }

        public required IReadOnlyList<AudioStream> AudioStreams { get; set; }
        public required IReadOnlyList<VideoStream> VideoStreams { get; set; }

        //public Engagement Engagement { get; } = engagement;

        //[ExcludeFromCodeCoverage]
        //public override string ToString() => $"Video ({Title})";
    }

    public class VideoStream
    {
        public required string Url { get; set; }
        public required string Container { get; set; }

        public required string Size { get; set; }

        public required string Bitrate { get; set; }

        public required string VideoCodec { get; set; }

        public  required string VideoQuality { get; set; }

        public required string VideoResolution { get; set; }

        public required string HashId { get; set; }

        public override string ToString() => $"Video-only ({VideoQuality} | {Container})";
    }

    public class AudioStream
    {
        public required string Url { get; set; }

        public required string Container { get; set; }

        public required string Size { get; set; }

        public required string Bitrate { get; set; }

        public required string AudioCodec { get; set; }

        public required string? AudioLanguage { get; set; }

        public required string? IsAudioLanguageDefault { get; set; }

        public required string HashId { get; set; }

        public override string ToString() => AudioLanguage is not null
                ? $"Audio-only ({Container} | {AudioLanguage})"
                : $"Audio-only ({Container})";
    }

}
