using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace RetroPOS.Consumer.Api.Functions
{
    public static class Consumer
    {
        [FunctionName("Consumer")]
        public static void Run([ServiceBusTrigger(Settings.CONSUMER_QUEUE_NAME, Connection = "ServiceBusConnectionString")] string myQueueItem, Int32 deliveryCount, DateTime enqueuedTimeUtc, string messageId, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            log.LogInformation($"EnqueuedTimeUtc={enqueuedTimeUtc}");
            log.LogInformation($"DeliveryCount={deliveryCount}");
            log.LogInformation($"MessageId={messageId}");
        }
    }
}