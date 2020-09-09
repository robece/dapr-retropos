using Newtonsoft.Json;
using System.Collections.Generic;

namespace RetroPOS.Warehouse.Api.Models
{
    public class ProductsResult
    {
        [JsonProperty("products")]
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
