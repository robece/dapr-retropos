using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RetroPOS.Consumer.Api.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroPOS.Consumer.Api.Functions
{
    public static class Consumer
    {
        [FunctionName("Consumer")]
        public static async Task Run([ServiceBusTrigger(Settings.CONSUMER_QUEUE_NAME, Connection = "ServiceBusConnectionString")] string myQueueItem, Int32 deliveryCount, DateTime enqueuedTimeUtc, string messageId, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            log.LogInformation($"EnqueuedTimeUtc={enqueuedTimeUtc}");
            log.LogInformation($"DeliveryCount={deliveryCount}");
            log.LogInformation($"MessageId={messageId}");

            var product = JsonConvert.DeserializeObject<Product>(myQueueItem);

            var keyValueContent = new[]{ new{ key= messageId, value= product } };

            var jsonPayload = JsonConvert.SerializeObject(keyValueContent);
            var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(1);
            await httpClient.PostAsync($"http://localhost:{Settings.DAPR_HTTP_PORT}/v1.0/state/{Settings.CONSUMER_STATE_COMPONENT_NAME}", stringContent).ContinueWith((b) => { 
            
                if (b.Exception != null)
                {
                    log.LogError($"Error={b.Exception.Message}");
                } else
                {
                    if (b.Result.IsSuccessStatusCode)
                    {
                        log.LogInformation($"Success=true");
                    }
                    else
                    {
                        log.LogInformation($"Success=false");
                    }
                }
            });
        }
    }
}