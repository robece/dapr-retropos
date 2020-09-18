using Newtonsoft.Json;

namespace RetroPOS.DurableOrchestration.Api.Models
{
    public class OrchestrationResult
    {
        [JsonProperty("successTotal")]
        public int SuccessTotal { get; set; }

        [JsonProperty("failuresTotal")]
        public int FailuresTotal { get; set; }
    }
}
