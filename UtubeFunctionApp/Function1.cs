using System;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UtubeFunctionApp
{
    public class Function12
    {
        private readonly ILogger<Function1> _logger;

        public Function12(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function12))]
        //public async Task Run([QueueTrigger("ut-download-finished", Connection = "StorageConnectionString", AutoComplete = false)] QueueMessage msg)
        public void Run([QueueTrigger("ut-download-finished", Connection = "AzuriteStorageConnection")] QueueMessage msg)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {msg.MessageText}");
        }
    }

    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly QueueClient _queueClient;

        public Function1(ILogger<Function1> logger, QueueClient queueClient)
        {
            _logger = logger;
            _queueClient = queueClient;
        }

        [Function(nameof(Function1))]
        public async Task Run([QueueTrigger("ut-download-finished", Connection = "AzuriteStorageConnection")] QueueMessage msg)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {msg.MessageText}");

            try
            {
                // Process the message
                var newMessage = $"Processed message: {msg.MessageText}";
                await _queueClient.SendMessageAsync(newMessage);
                _logger.LogInformation("New message added to the queue.");

                // Delete the message from the queue
                await _queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
                _logger.LogInformation("Message deleted from the queue.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the message.");
                // Optionally, you can handle the error and decide whether to delete the message or not
            }
        }
    }
}
