using Microsoft.AspNetCore.Mvc;
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
}
