//using System;
//using Azure.Storage.Queues.Models;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Logging;

//namespace UtubeFunctionApp
//{
//    public class UDownLoad 
//    {
//        private readonly ILogger<UDownLoad> _logger;

//        public UDownLoad(ILogger<UDownLoad> logger)
//        {
//            _logger = logger;
//        }

//        [Function(nameof(UDownLoad))]
//        public void Run([QueueTrigger("myqueue-items", Connection = "AzuriteStorageConnection")] QueueMessage message)
//        {
//            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");
//        }
//    }
//}
