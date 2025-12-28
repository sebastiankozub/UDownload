using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Globalization;

namespace UtubeRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MediaController : ControllerBase
{
    private static readonly string MediaRoot = "/home/app/downloads";

    // GET api/media/stream?path=relative/path/to/file.mp4
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
        long start = 0; long end = fileInfo.Length - 1;
        bool isRangeRequest = false;

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

        // Copy only the requested range to the response to avoid clients (like Swagger UI) hanging on full-file streams
        const int bufferSize = 64 * 1024; // 64KB
        var buffer = new byte[bufferSize];
        long remaining = length;
        while (remaining > 0)
        {
            var toRead = (int)Math.Min(bufferSize, remaining);
            var read = await stream.ReadAsync(buffer.AsMemory(0, toRead));
            if (read <= 0) break;
            await Response.Body.WriteAsync(buffer.AsMemory(0, read));
            remaining -= read;
        }

        return new EmptyResult();
    }

    private static bool TryParseRange(string rangeHeader, long fileLength, out long start, out long end)
    {
        start = 0; end = fileLength - 1;
        // format: bytes=start-end or bytes=start- or bytes=-suffixLength
        var parts = rangeHeader.Substring("bytes=".Length).Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;
        var range = parts[0].Trim();
        var dashIndex = range.IndexOf('-');
        if (dashIndex < 0) return false;
        var startStr = range[..dashIndex].Trim();
        var endStr = range[(dashIndex + 1)..].Trim();

        if (startStr.Length == 0 && endStr.Length > 0)
        {
            if (!long.TryParse(endStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var suffix)) return false;
            if (suffix <= 0) return false;
            start = Math.Max(fileLength - suffix, 0);
            end = fileLength - 1;
            return true;
        }

        if (!long.TryParse(startStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out start)) return false;
        if (endStr.Length == 0)
        {
            end = fileLength - 1;
            return true;
        }
        if (!long.TryParse(endStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out end)) return false;
        if (start < 0 || end >= fileLength || start > end) return false;
        return true;
    }

    private static string GetContentType(string ext)
    {
        switch (ext.ToLowerInvariant())
        {
            case ".mp4": return "video/mp4";
            case ".webm": return "video/webm";
            case ".ogg": return "video/ogg";
            case ".mp3": return "audio/mpeg";
            case ".wav": return "audio/wav";
            case ".m4a": return "audio/mp4";
            default: return "application/octet-stream";
        }
    }
}
