using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UtubeRest.Service;


public class AvYtManifest
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("title")]
    public required string Title { get; set; }

    [JsonPropertyName("formats")]
    public required List<AvYtFormatManifest> Formats { get; set; }

    [JsonPropertyName("thumbnails")]
    public List<Thumbnail>? Thumbnails { get; set; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("channel_url")]
    public string? ChannelUrl { get; set; }

    [JsonPropertyName("duration")]
    public required int Duration { get; set; }

    [JsonPropertyName("view_count")]
    public int? ViewCount { get; set; }

    [JsonPropertyName("average_rating")]
    public object? AverageRating { get; set; }

    [JsonPropertyName("age_limit")]
    public int? AgeLimit { get; set; }

    [JsonPropertyName("webpage_url")]
    public string? WebpageUrl { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("playable_in_embed")]
    public bool? PlayableInEmbed { get; set; }

    [JsonPropertyName("live_status")]
    public string? LiveStatus { get; set; }

    [JsonPropertyName("release_timestamp")]
    public long? ReleaseTimestamp { get; set; }

    [JsonPropertyName("_format_sort_fields")]
    public List<string>? FormatSortFields { get; set; }

    [JsonPropertyName("automatic_captions")]
    public Dictionary<string, List<Caption>>? AutomaticCaptions { get; set; }
}

public class AvYtFormatManifest
{
    [JsonPropertyName("format_id")]
    public required string FormatId { get; set; }

    [JsonPropertyName("format_note")]
    public string? FormatNote { get; set; }

    [JsonPropertyName("ext")]
    public string? Ext { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("acodec")]
    public string? Acodec { get; set; }

    [JsonPropertyName("vcodec")]
    public string? Vcodec { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("fps")]
    public double? Fps { get; set; }

    [JsonPropertyName("rows")]
    public int? Rows { get; set; }

    [JsonPropertyName("columns")]
    public int? Columns { get; set; }

    [JsonPropertyName("fragments")]
    public List<Fragment>? Fragments { get; set; }

    [JsonPropertyName("audio_ext")]
    public string? AudioExt { get; set; }

    [JsonPropertyName("video_ext")]
    public string? VideoExt { get; set; }

    [JsonPropertyName("vbr")]
    public float? Vbr { get; set; }

    [JsonPropertyName("abr")]
    public float? Abr { get; set; }

    [JsonPropertyName("tbr")]
    public object? Tbr { get; set; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("aspect_ratio")]
    public double? AspectRatio { get; set; }

    [JsonPropertyName("filesize_approx")]
    public object? FilesizeApprox { get; set; }

    [JsonPropertyName("http_headers")]
    public HttpHeaders? HttpHeaders { get; set; }

    [JsonPropertyName("format")]
    public string? FormatDescription { get; set; }

    [JsonPropertyName("asr")]
    public int? Asr { get; set; }

    [JsonPropertyName("filesize")]
    public long? Filesize { get; set; }

    [JsonPropertyName("source_preference")]
    public int? SourcePreference { get; set; }

    [JsonPropertyName("audio_channels")]
    public int? AudioChannels { get; set; }

    [JsonPropertyName("quality")]
    public double? Quality { get; set; }

    [JsonPropertyName("has_drm")]
    public bool? HasDrm { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("language_preference")]
    public int? LanguagePreference { get; set; }

    [JsonPropertyName("preference")]
    public object? Preference { get; set; }

    [JsonPropertyName("container")]
    public string? Container { get; set; }

    [JsonPropertyName("downloader_options")]
    public DownloaderOptions? DownloaderOptions { get; set; }

    [JsonPropertyName("dynamic_range")]
    public string? DynamicRange { get; set; }
}

public class Fragment
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("duration")]
    public required double Duration { get; set; }
}

public class HttpHeaders
{
    [JsonPropertyName("User-Agent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("Accept")]
    public string? Accept { get; set; }

    [JsonPropertyName("Accept-Language")]
    public string? AcceptLanguage { get; set; }

    [JsonPropertyName("Sec-Fetch-Mode")]
    public string? SecFetchMode { get; set; }
}

public class DownloaderOptions
{
    [JsonPropertyName("http_chunk_size")]
    public int? HttpChunkSize { get; set; }
}

public class Thumbnail
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("preference")]
    public int? Preference { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

public class Caption
{
    [JsonPropertyName("ext")]
    public required string Ext { get; set; }

    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

