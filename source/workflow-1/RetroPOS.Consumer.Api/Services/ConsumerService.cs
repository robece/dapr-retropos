using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroPOS.Consumer.Api.Services
{
    public class ConsumerService : IConsumerService
    {
        private readonly HttpClient httpClient;

        public ConsumerService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> SendToStateAsync(string jsonPayload, ILogger logger)
        {
            bool result = false;

            try
            {
                var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                logger.LogInformation("******* Using Dapr sidecar *******");
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"http://localhost:{Settings.DAPR_HTTP_PORT}/v1.0/state/{Settings.CONSUMER_STATE_COMPONENT_NAME}", stringContent);
                httpResponseMessage.EnsureSuccessStatusCode();

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
