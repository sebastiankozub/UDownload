using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UtubeFunctionApp
{
    public class Function3
    {
        private readonly ILogger<Function3> _logger;

        public Function3(ILogger<Function3> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function3))]
        public async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "AzuriteStorageConnection")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }
    }
}
