using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RetroPOS.Consumer.Function.Models;
using RetroPOS.Consumer.Function.Services;
using System;
using System.Threading.Tasks;

namespace RetroPOS.Consumer.Function.Activities
{
    public class Consumer
    {
        private readonly IConsumerService consumerSvc;

        public Consumer(IConsumerService consumerSvc)
        {
            this.consumerSvc = consumerSvc;
        }

        [FunctionName("Consumer")]
        public async Task Run([ServiceBusTrigger(Settings.CONSUMER_QUEUE_NAME, Connection = "ServiceBusConnectionString")] Message message, MessageReceiver messageReceiver, ILogger logger)
        {
            try
            {
                var product = JsonConvert.DeserializeObject<Product>(System.Text.Encoding.UTF8.GetString(message.Body));
                var keyValueContent = new[] { new { key = message.MessageId, value = product } };
                var jsonPayload = JsonConvert.SerializeObject(keyValueContent);

                var result = await consumerSvc.SendToStateAsync(jsonPayload, logger);

                if (result)
                {
                    logger.LogInformation("Message processed successfully");
                    await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
                }
                else
                {
                    logger.LogInformation("Message has been abandoned and moved to the queue again");
                    await messageReceiver.AbandonAsync(message.SystemProperties.LockToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                await messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken);
            }
        }
    }
}