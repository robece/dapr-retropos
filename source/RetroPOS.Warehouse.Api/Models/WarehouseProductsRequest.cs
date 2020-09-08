using Newtonsoft.Json;

namespace RetroPOS.Warehouse.Api.Models
{
    public class WarehouseProductsRequest
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }
    }
}
