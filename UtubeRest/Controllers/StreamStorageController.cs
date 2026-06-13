using Microsoft.AspNetCore.Mvc;
using UtubeRest.Service;

// https://go.microsoft.com/fwlink/?LinkID=397860

namespace UtubeRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StreamStorageController : ControllerBase
{
    private readonly IStreamDownloadQueue _streamDownloadQueue;

    public StreamStorageController(IStreamDownloadQueue streamDownloadQueue)
    {
        _streamDownloadQueue = streamDownloadQueue;
    }



    // POST api/<StreamStorageController>/Import
    [HttpPost("Import")]
    public async Task<IActionResult> PostImport([FromBody] StreamImportRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.VideoId))
        {
            return BadRequest("VideoId is required.");
        }

        if (request.AudioHashIds.Count != 1)
        {
            return BadRequest("Exactly one audio stream must be selected.");
        }

        if (request.VideoHashIds.Count != 1)
        {
            return BadRequest("Exactly one video stream must be selected.");
        }

        var jobId = Guid.NewGuid().ToString("N");

        await _streamDownloadQueue.QueueAsync(
            new QueuedStreamDownloadRequest(
                jobId,
                request.VideoId,
                request.AudioHashIds[0],
                request.VideoHashIds[0]),
            cancellationToken);

        return Accepted(new
        {
            jobId,
            status = "queued",
            videoId = request.VideoId,
        });
    }


    //// GET: api/<StreamStorageController>
    //[HttpGet]
    //public IEnumerable<string> Get()
    //{
    //    return new string[] { "value1", "value2" };
    //}

    //// GET api/<StreamStorageController>/5
    //[HttpGet("{id}")]
    //public string Get(int id)
    //{
    //    return "value";
    //}

    //// POST api/<StreamStorageController>
    //[HttpPost]
    //public void Post([FromBody] string value)
    //{
    //}

    //// PUT api/<StreamStorageController>/5
    //[HttpPut("{id}")]
    //public void Put(int id, [FromBody] string value)
    //{
    //}

    //// DELETE api/<StreamStorageController>/5
    //[HttpDelete("{id}")]
    //public void Delete(int id)
    //{
    //}
}

public sealed class StreamImportRequest
{
    public string VideoId { get; set; } = string.Empty;
    public List<string> AudioHashIds { get; set; } = [];
    public List<string> VideoHashIds { get; set; } = [];
}
