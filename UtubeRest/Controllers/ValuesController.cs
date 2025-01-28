using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using UtubeRest.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UtubeRest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET: api/<ValuesController>
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var ffMpegVersionCommmand = "ffmpeg -version";
            var ytDlpDownloadCommand = "yt-dlp - f 18 ZV5yTm4pT8g"; // "yt-dlp --version";

            var ffmepegVersion = await YtdlpService.RunUnixCommandAsync(ffMpegVersionCommmand);


            var ytDlpVersion = await YtdlpService.RunUnixCommandAsync(ytDlpDownloadCommand);

            var us = new YtdlpService();
            var avManifest = us.GetAvManifest("https://www.youtube.com/watch?v=6n3pFFPSlW4");


            var urls = us.GetAvFormats("https://www.youtube.com/watch?v=6n3pFFPSlW4");

            string s = "";
            foreach(var url in urls)
            {
                s += " \n " + url.Url;

            }


            var dynamicRes = JsonConvert.DeserializeObject(avManifest);  // Newtonsoft



            return new string[] { avManifest };
        }


        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
