using Newtonsoft.Json;

namespace RetroPOS.DurableOrchestration.Api.Models
{
    public class SubOrchestrationResult
    {
        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("success")]
        public int Success { get; set; }

        [JsonProperty("failures")]
        public int Failures { get; set; }
    }
}
