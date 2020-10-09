using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RetroPOS.Publisher.Api.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetroPOS.Publisher.Api.Services
{
    public class PublisherService : IPublisherService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<PublisherService> logger;

        public PublisherService(HttpClient httpClient, ILogger<PublisherService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<bool> NotifySubscriptors(InternalMessage message)
        {
            bool result = false;

            try
            {
                var jsonPayload = JsonConvert.SerializeObject(message);

                logger.LogInformation("******* Using Dapr sidecar *******");
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage() { Method = HttpMethod.Post, RequestUri = new Uri($"http://localhost:{Settings.DAPR_HTTP_PORT}/v1.0/publish/{Settings.PUBSUB_COMPONENT_NAME}/{Settings.PUBSUB_TOPIC_NAME}"), Content = new StringContent(jsonPayload) };
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