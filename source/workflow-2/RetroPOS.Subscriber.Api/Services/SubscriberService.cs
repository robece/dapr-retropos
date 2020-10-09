using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RetroPOS.Subscriber.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetroPOS.Subscriber.Api.Services
{
    public class SubscriberService : ISubscriberService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<SubscriberService> logger;

        public SubscriberService(HttpClient httpClient, ILogger<SubscriberService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<bool> SendToOutputBinding(InternalMessage message)
        {
            bool result = false;

            try
            {
                var metadata = new Dictionary<string, string>();
                metadata.Add("blobName", $"{message.MessageId}.json");
                metadata.Add("ContentType", "application/json");

                var payload = new
                {
                    data = JsonConvert.SerializeObject(message),
                    metadata = metadata,
                    operation = "create"
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);

                logger.LogInformation("******* Using Dapr sidecar *******");
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() { Method = HttpMethod.Post, RequestUri = new Uri($"http://localhost:{Settings.DAPR_HTTP_PORT}/v1.0/bindings/{Settings.OUTPUT_BINDING_COMPONENT_NAME}"), Content = new StringContent(jsonPayload) };
                HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                    result = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            return result;
        }
    }
}