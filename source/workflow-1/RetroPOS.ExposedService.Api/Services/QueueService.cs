using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RetroPOS.ExposedService.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetroPOS.ExposedService.Api.Services
{
    public class QueueService : IQueueService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<QueueService> logger;

        public QueueService(HttpClient httpClient, ILogger<QueueService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task<bool> QueueMessageAsync()
        {
            bool result = false;

            try
            {
                var id = Guid.NewGuid().ToString();
                var entity = new Product()
                {
                    ProductId = id,
                    ProductDescription = $"product description - {id}"
                };

                var payload = new
                {
                    data = entity,
                    operation = "create"
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var headers = new Dictionary<string, StringValues>() { { "Content-Type", "application/json" } };

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