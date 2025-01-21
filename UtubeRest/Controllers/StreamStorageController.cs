using Microsoft.AspNetCore.Mvc;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkID=397860

namespace UtubeRest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StreamStorageController : ControllerBase
{

    // POST api/<StreamStorageController>/Import
    [HttpPost("Import")]
    public async Task<IActionResult> PostImport([FromBody] string[] hashIds)
    {
        // create job id


        // table storage stream-import-jobs - job id + hashes

        // table storage stream-import-status - hash + status=queued

        string rawContent = string.Empty;
        using var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false);
        rawContent = await reader.ReadToEndAsync();
        //push to table storage;
        //return 202

        return Accepted(); // with status location header
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

