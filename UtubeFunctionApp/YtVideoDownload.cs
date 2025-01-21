//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Azure.Storage.Blobs;
//using YoutubeExplode;
//using System.Linq;
//using YoutubeExplode.Videos;
//using ApiFetchAndCacheApp.Options;
//using Microsoft.Azure.Functions.Worker.Http;
//using System.Net;
//using Azure.Storage.Queues;
//using System.Text;


//namespace UtubeFunctionApp;

//public class YtVideoDownload
//{
//    private readonly ILogger<YtVideoDownload> _logger;
//    private readonly string _storageConnectionString;
//    private readonly string _storageContainerName;

//    //private readonly PublicApiOptions _publicApiOptions;
//    //private readonly PayloadStorageOptions _payloadStorageOptions;
//    //private readonly LogStorageOptions _logStorageOptions;

//    public YtVideoDownload(ILogger<YtVideoDownload> logger, IConfiguration configuration, PublicApiOptions publicApiOptions,
//            PayloadStorageOptions payloadStorageOptions,    LogStorageOptions logStorageOptions)
//    {

//            _publicApiOptions = publicApiOptions;
//            _payloadStorageOptions = payloadStorageOptions;
//            _logStorageOptions = logStorageOptions;


//        _logger = logger;
//        _storageConnectionString = configuration["BlobStorageConnectionString"]; // Get from configuration
//        _storageContainerName = configuration["BlobStorageContainerName"]; // Get from configuration
//    }

//    [Function("YtVideoDownload")]
//    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "download/{videoUrl}")] HttpRequestData req, string videoUrl)
//    {
//        var startTime = DateTime.Now;
//        _logger.LogInformation($"YtVideoDownload starts processing... : {startTime}");
//        //string videoUrl = await new StreamReader(req.Body).ReadToEndAsync();

//        if (string.IsNullOrEmpty(videoUrl))
//        {
//            //req.HttpResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
//            //await req.WriteStringAsync("Please provide a YouTube video URL in the request body.");
//            var response = req.CreateResponse(HttpStatusCode.OK);
//            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
//            response.WriteString("ok-json");
//            return response;
//        }

//        try
//        {
//            var youtube = new YoutubeClient();
//            var videoId = YoutubeExplode.Videos.VideoId.Parse(videoUrl);

//            var videoIdManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

//            var videoStreamInfos = videoIdManifest.GetVideoOnlyStreams();
//            var audioStreamInfos = videoIdManifest.GetAudioOnlyStreams();

//            var videoStreamInfo = videoStreamInfos.First();
//            var audioStreamInfo = audioStreamInfos.First();   // last is the best

            
//            var streamInfo = videoIdManifest.GetMuxedStreams();  // deprecated

//            if (videoStreamInfo is not null)
//            {
//                var streamInfo2 = videoStreamInfo;

//                var stream = await youtube.Videos.Streams.GetAsync(streamInfo2);
//                var video = await youtube.Videos.GetAsync(videoId);

//                var fileName = $"{video.Title}.{streamInfo2.Container}";

//                fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"[^a-zA-Z0-9\.\-]+", "-");

//                // Upload to Blob Storage
//                var blobServiceClient = new BlobServiceClient(_payloadStorageOptions.StorageConnectionString);
//                var containerClient = blobServiceClient.GetBlobContainerClient(_payloadStorageOptions.Container+"-videos");
//                await containerClient.CreateIfNotExistsAsync();

//                var blobClient = containerClient.GetBlobClient(fileName);

//                await blobClient.UploadAsync(stream, overwrite: true);
//                // await blobClient.UploadAsync(stream);


//                if (audioStreamInfo is not null)
//                {

//                    var streamInfo3 = audioStreamInfo;
//                    var stream3 = await youtube.Videos.Streams.GetAsync(streamInfo3);
//                    var video3 = await youtube.Videos.GetAsync(videoId);
//                    var fileName3 = $"{video3.Title}-audio.{streamInfo3.Container}";
//                    fileName3 = System.Text.RegularExpressions.Regex.Replace(fileName3, @"[^a-zA-Z0-9\.\-]+", "-");
//                    // Upload to Blob Storage
//                    var blobServiceClient3 = new BlobServiceClient(_payloadStorageOptions.StorageConnectionString);
//                    var containerClient3 = blobServiceClient3.GetBlobContainerClient(_payloadStorageOptions.Container + "-audios");
//                    await containerClient3.CreateIfNotExistsAsync();
//                    var blobClient3 = containerClient3.GetBlobClient(fileName3);
//                    await blobClient3.UploadAsync(stream3, overwrite: true);
//                    // await blobClient.UploadAsync(stream);

//                }

//                // save to queue
//                var rnd = new Random(987);
//                var rndNr = rnd.Next(900);

//                var queueClient = new QueueClient(_payloadStorageOptions.StorageConnectionString, "ut-download-finished");
//                await queueClient.CreateIfNotExistsAsync();
//                var message = $"{videoUrl}    --   Video & Sound  --   '{video.Title}'    --    has been downloaded --{rndNr}";
//                var bytes = Encoding.UTF8.GetBytes(message);
//                await queueClient.SendMessageAsync(Convert.ToBase64String(bytes));
//                await queueClient.SendMessageAsync(Convert.ToBase64String(bytes));
//                await queueClient.SendMessageAsync(Convert.ToBase64String(bytes));

//                // http
//                var response = req.CreateResponse(HttpStatusCode.OK);
//                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
//                await response.WriteStringAsync("ok-json files downloaded");
//                return response;    
//            }
//            else
//            {


//                throw new Exception("No stream available for the video.");
//            }

//        }
//        catch (YoutubeExplode.Exceptions.VideoUnavailableException ex)
//        {
//            _logger.LogError(ex, "Video unavailable: " + videoUrl);
//            //req.HttpResponse.StatusCode = System.Net.HttpStatusCode.NotFound;
//            //await req.WriteStringAsync($"Video '{videoUrl}' is unavailable.");
//            _logger.LogInformation($"Problem communicating underlying system");
//            return req.CreateResponse(HttpStatusCode.NotFound);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error downloading or uploading video: " + videoUrl);
//            //req.HttpResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
//            //await req.WriteStringAsync("An error occurred while processing the video.");
//            _logger.LogInformation($"Problem communicating underlying system");
//            return req.CreateResponse(HttpStatusCode.InternalServerError);
//        }
//        finally
//        {
//            var tookTime = DateTime.Now - startTime;
//            _logger.LogInformation($"YtVideoDownload finished processing... : {tookTime}");
//        }
//    }    
//}
