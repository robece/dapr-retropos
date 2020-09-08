using Newtonsoft.Json;
using System.Collections.Generic;

namespace RetroPOS.Warehouse.Api.Models
{
    public class WarehouseProductsResult
    {
        [JsonProperty("products")]
        public List<WarehouseProduct> Products { get; set; } = new List<WarehouseProduct>();
    }
}
