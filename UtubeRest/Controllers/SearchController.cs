using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UtubeRest.Service;
using UtubeRest.ViewModel;

namespace UtubeRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly YtService _ytService;

    public SearchController(YtService ytService)
    {
        _ytService = ytService;
    }

    // GET api/search?q=some query&count=4
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SearchResult>>> Get([FromQuery] string q, [FromQuery] int count = 4)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Missing q");

        if (count <= 0 || count > 10000)
            count = 10000;

        var results = await _ytService.SearchAsync(q, count);
        return Ok(results);
    }

    [HttpGet("stream")]
    public async Task Stream([FromQuery] string q, [FromQuery] int count = 4, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("Missing q", cancellationToken);
            return;
        }

        if (count <= 0 || count > 10000)
            count = 10000;

        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await Response.WriteAsync(": search stream\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        try
        {
            await _ytService.SearchStreamAsync(
                q,
                count,
                async result =>
                {
                    var payload = JsonSerializer.Serialize(result);
                    await WriteSseMessageAsync(payload, cancellationToken);
                },
                cancellationToken);

            await WriteSseMessageAsync("complete", cancellationToken, "done");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task WriteSseMessageAsync(string data, CancellationToken cancellationToken, string? eventName = null)
    {
        if (!string.IsNullOrEmpty(eventName))
        {
            await Response.WriteAsync($"event: {eventName}\n", cancellationToken);
        }

        foreach (var line in data.Split('\n'))
        {
            await Response.WriteAsync($"data: {line}\n", cancellationToken);
        }

        await Response.WriteAsync("\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
}
