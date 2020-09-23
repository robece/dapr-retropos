using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RetroPOS.DurableOrchestration.Api.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetroPOS.DurableOrchestration.Api.Functions
{
    public class SubOrchestration
    {
        [FunctionName("SubOrchestration")]
        public async Task<SubOrchestrationResult> Run([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            SubOrchestrationResult result = new SubOrchestrationResult();
            int maxRequestsPerWorkload = Settings.MAX_REQUESTS_PER_WORKLOAD;

            result.InstanceId = context.InstanceId;

            var input = context.GetInput<string>();
            JObject json = JObject.Parse(input);
            string enumerator = json.Value<string>("enumerator").ToString();

            var tasks = new List<Task<DurableHttpResponse>>();

            for (int i = 0; i < maxRequestsPerWorkload; i++)
            {
                var entity = new
                {
                    productId = i,
                    productDescription = $"workload: {enumerator} - index: {i}"
                };

                var payload = new
                {
                    data = JsonConvert.SerializeObject(entity),
                    operation = "create"
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var headers = new Dictionary<string, StringValues>() { { "Content-Type", "application/json" } };
                DurableHttpRequest durableHttpRequest = new DurableHttpRequest(HttpMethod.Post, new Uri($"http://localhost:{Settings.DAPR_HTTP_PORT}/v1.0/bindings/{Settings.DURABLE_BINDING_COMPONENT_NAME}"), headers, jsonPayload);

                tasks.Add(context.CallHttpAsync(durableHttpRequest).ContinueWith<DurableHttpResponse>((response) =>
                {
                    if (response.Exception != null)
                    {
                        result.Failures++;
                    }
                    else
                    {
                        if (response.Result.StatusCode == HttpStatusCode.OK)
                        {
                            result.Success++;
                        }
                        else
                        {
                            result.Failures++;
                        }
                    }

                    return response.Result;
                }));
            }

            await Task.WhenAll(tasks);

            log.LogInformation($"Success requests total: { result.Success}");

            return result;
        }
    }
}
