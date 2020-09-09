using Newtonsoft.Json;

namespace RetroPOS.Warehouse.Api.Models
{
    public class ProductsRequest
    {
        [JsonProperty("warehouseID")]
        public string WarehouseID { get; set; }
    }
}
