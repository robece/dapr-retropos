using Newtonsoft.Json;

namespace RetroPOS.Publisher.Api.Models
{
    public class InternalMessage
    {
        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
