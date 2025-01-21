using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UtubeFunctionApp
{
    public class Function4
    {
        private readonly ILogger<Function4> _logger;

        public Function4(ILogger<Function4> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function4))]
        public void Run([QueueTrigger("myqueue-items", Connection = "AzuriteStorageConnection")] QueueMessage message)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
        }
    }
}
