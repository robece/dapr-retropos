using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RetroPOS.DurableOrchestration.Api.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RetroPOS.DurableOrchestration.Api.Functions
{
    public class Orchestration
    {
        [FunctionName("Orchestration")]
        public async Task<OrchestrationResult> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var result = new OrchestrationResult();
            var tasks = new List<Task<SubOrchestrationResult>>();
            int workloads = Settings.MAX_WORKLOADS;

            for (int i = 0; i < workloads; i++)
            {
                var payload = new
                {
                    enumerator = i
                };

                Task<SubOrchestrationResult> t = context.CallSubOrchestratorAsync<SubOrchestrationResult>("SubOrchestration", JsonConvert.SerializeObject(payload));
                tasks.Add(t);
            }

            await Task.WhenAll(tasks);

            foreach (Task<SubOrchestrationResult> t in tasks)
            {
                result.SuccessTotal += t.Result.Success;
                result.FailuresTotal += t.Result.Failures;
            }

            return result;
        }

        [FunctionName("Orchestration_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Orchestration", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}