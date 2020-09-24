using Newtonsoft.Json;

namespace RetroPOS.DurableOrchestration.Api.Models
{
    public class Product
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productDescription")]
        public string ProductDescription { get; set; }
    }
}
