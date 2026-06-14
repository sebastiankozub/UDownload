using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using UtubeRest.Service;

// https://go.microsoft.com/fwlink/?LinkID=397860

namespace UtubeRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StreamStorageController : ControllerBase
{
    private static readonly string MediaRoot = "/home/app/downloads";
    private readonly IStreamDownloadQueue _streamDownloadQueue;

    public StreamStorageController(IStreamDownloadQueue streamDownloadQueue)
    {
        _streamDownloadQueue = streamDownloadQueue;
    }

    [HttpGet("files")]
    public ActionResult<IEnumerable<DownloadedFileItem>> GetFiles()
    {
        var baseDir = Path.GetFullPath(MediaRoot);
        if (!Directory.Exists(baseDir))
        {
            return Ok(Array.Empty<DownloadedFileItem>());
        }

        var files = Directory
            .EnumerateFiles(baseDir, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists && !file.Name.StartsWith('.'))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new DownloadedFileItem
            {
                Name = file.Name,
                Path = Path.GetRelativePath(baseDir, file.FullName).Replace('\\', '/'),
                SizeBytes = file.Length,
                ModifiedAtUtc = file.LastWriteTimeUtc,
                ContentType = GetContentType(file.Extension),
                IsPlayable = IsPlayableExtension(file.Extension),
            })
            .ToList();

        return Ok(files);
    }

    [HttpGet("stream")]
    public async Task<IActionResult> Stream([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Missing path");

        var baseDir = Path.GetFullPath(MediaRoot);
        var fullPath = Path.GetFullPath(Path.Combine(baseDir, path));
        if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid path");

        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var fileInfo = new FileInfo(fullPath);
        var contentType = GetContentType(fileInfo.Extension);

        var rangeHeader = Request.Headers[HeaderNames.Range].ToString();
        long start = 0;
        long end = fileInfo.Length - 1;
        var isRangeRequest = false;

        if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
        {
            isRangeRequest = TryParseRange(rangeHeader, fileInfo.Length, out start, out end);
        }

        Response.Headers[HeaderNames.AcceptRanges] = "bytes";
        Response.Headers[HeaderNames.CacheControl] = "no-cache";

        var length = end - start + 1;
        await using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(start, SeekOrigin.Begin);

        if (isRangeRequest)
        {
            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.Headers[HeaderNames.ContentRange] = $"bytes {start}-{end}/{fileInfo.Length}";
        }

        Response.ContentType = contentType;
        Response.ContentLength = length;

        const int bufferSize = 64 * 1024;
        var buffer = new byte[bufferSize];
        long remaining = length;
        while (remaining > 0)
        {
            var toRead = (int)Math.Min(bufferSize, remaining);
            var read = await stream.ReadAsync(buffer.AsMemory(0, toRead));
            if (read <= 0)
            {
                break;
            }

            await Response.Body.WriteAsync(buffer.AsMemory(0, read));
            remaining -= read;
        }

        return new EmptyResult();
    }


    // POST api/<StreamStorageController>/Import
    [HttpPost("Import")]
    public async Task<IActionResult> PostImport([FromBody] StreamImportRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VideoId))
        {
            return BadRequest("VideoId is required.");
        }

        if (request.AudioFormatIds.Count > 1)
        {
            return BadRequest("Only one audio stream can be selected.");
        }

        if (request.VideoFormatIds.Count > 1)
        {
            return BadRequest("Only one video stream can be selected.");
        }

        if (request.AudioFormatIds.Count == 0 && request.VideoFormatIds.Count == 0)
        {
            return BadRequest("Select at least one audio or video stream.");
        }

        var jobId = Guid.NewGuid().ToString("N");

        await _streamDownloadQueue.QueueAsync(
            new QueuedStreamDownloadRequest(
                jobId,
                request.VideoId,
                request.AudioFormatIds.FirstOrDefault(),
                request.VideoFormatIds.FirstOrDefault()),
            cancellationToken);

        return Accepted(new
        {
            jobId,
            status = "queued",
            videoId = request.VideoId,
            audioFormatId = request.AudioFormatIds.FirstOrDefault(),
            videoFormatId = request.VideoFormatIds.FirstOrDefault(),
        });
    }


    private static bool TryParseRange(string rangeHeader, long fileLength, out long start, out long end)
    {
        start = 0;
        end = fileLength - 1;

        var parts = rangeHeader.Substring("bytes=".Length).Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        var range = parts[0].Trim();
        var dashIndex = range.IndexOf('-');
        if (dashIndex < 0)
        {
            return false;
        }

        var startStr = range[..dashIndex].Trim();
        var endStr = range[(dashIndex + 1)..].Trim();

        if (startStr.Length == 0 && endStr.Length > 0)
        {
            if (!long.TryParse(endStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var suffix))
            {
                return false;
            }

            if (suffix <= 0)
            {
                return false;
            }

            start = Math.Max(fileLength - suffix, 0);
            end = fileLength - 1;
            return true;
        }

        if (!long.TryParse(startStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out start))
        {
            return false;
        }

        if (endStr.Length == 0)
        {
            end = fileLength - 1;
            return true;
        }

        if (!long.TryParse(endStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out end))
        {
            return false;
        }

        return start >= 0 && end < fileLength && start <= end;
    }

    private static string GetContentType(string ext)
    {
        switch (ext.ToLowerInvariant())
        {
            case ".mkv": return "video/x-matroska";
            case ".mp4": return "video/mp4";
            case ".webm": return "video/webm";
            case ".ogg": return "video/ogg";
            case ".mp3": return "audio/mpeg";
            case ".wav": return "audio/wav";
            case ".m4a": return "audio/mp4";
            default: return "application/octet-stream";
        }
    }

    private static bool IsPlayableExtension(string ext)
    {
        switch (ext.ToLowerInvariant())
        {
            case ".mkv":
            case ".mp4":
            case ".webm":
            case ".ogg":
            case ".mp3":
            case ".wav":
            case ".m4a":
                return true;
            default:
                return false;
        }
    }
}

public sealed class StreamImportRequest
{
    public string VideoId { get; set; } = string.Empty;
    public List<string> AudioFormatIds { get; set; } = [];
    public List<string> VideoFormatIds { get; set; } = [];
}

public sealed class DownloadedFileItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime ModifiedAtUtc { get; set; }
    public string ContentType { get; set; } = "application/octet-stream";
    public bool IsPlayable { get; set; }
}
