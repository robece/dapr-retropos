using Newtonsoft.Json;
using RetroPOS.Audit.Api;
using RetroPOS.Warehouse.Api.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RetroPOS.Warehouse.Api.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly HttpClient httpClient;

        public WarehouseService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<bool> ProductUpdateRegistration(ProductUpdateRegistrationRequest request)
        {
            bool result = false;
            List<WarehouseProduct> list = null;
            var products = await GetWarehouseProducts(request.WarehouseID);

            if (products == null)
            {
                list = new List<WarehouseProduct>();
                list.Add(request);
            }
            else
            {
                list = products;
                var product = list.Find(x => x.ProductID == request.ProductID);

                if (product == null)
                {
                    list.Add(request);
                }
                else
                {
                    list.Remove(product);
                    list.Add(request);
                }
            }

            var keyValueContent = new[]{
                    new{ key= request.WarehouseID, value= list }
                };

            var jsonPayload = JsonConvert.SerializeObject(keyValueContent);
            var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Settings.DAPR_SIDECAR_BASEURL}/v1.0/state/{Settings.WAREHOUSE_STATE_COMPONENT_NAME}", stringContent);

            if (response.IsSuccessStatusCode)
                result = true;

            return result;
        }

        public async Task<bool> ProductDeletion(ProductDeletionRequest request)
        {
            bool result = false;
            List<WarehouseProduct> list = null;
            var products = await GetWarehouseProducts(request.WarehouseID);

            if (products != null)
            {
                list = products;
                var product = list.Find(x => x.ProductID == request.ProductID);

                if (product == null)
                {
                    return false;
                }
                else
                {
                    list.Remove(product);
                }

                var keyValueContent = new[]{
                    new{ key= request.WarehouseID, value= list }
                };

                var jsonPayload = JsonConvert.SerializeObject(keyValueContent);
                var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{Settings.DAPR_SIDECAR_BASEURL}/v1.0/state/{Settings.WAREHOUSE_STATE_COMPONENT_NAME}", stringContent);

                if (response.IsSuccessStatusCode)
                    result = true;
            }

            return result;
        }

        public async Task<bool> NotifySubscriptors(ProductUpdateRegistrationRequest request)
        {
            bool result = false;

            var jsonPayload = JsonConvert.SerializeObject(request);
            var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{Settings.DAPR_SIDECAR_BASEURL}/v1.0/publish/{Settings.PRODUCTREGISTRATION_PUBSUB_COMPONENT_NAME}/{Settings.PRODUCTREGISTRATION_PUBSUB_TOPIC_NAME}", stringContent);

            if (response.IsSuccessStatusCode)
                result = true;

            return result;
        }

        public async Task<WarehouseProductsResult> WarehouseProducts(WarehouseProductsRequest request)
        {
            WarehouseProductsResult result = new WarehouseProductsResult();
            result.Products = await GetWarehouseProducts(request.WarehouseID);

            if (result.Products == null)
                result.Products = new List<WarehouseProduct>();

            return result;
        }

        async Task<List<WarehouseProduct>> GetWarehouseProducts(string warehouseID)
        {
            List<WarehouseProduct> result = null;
            var response = await httpClient.GetAsync($"{Settings.DAPR_SIDECAR_BASEURL}/v1.0/state/{Settings.WAREHOUSE_STATE_COMPONENT_NAME}/{warehouseID}");

            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<List<WarehouseProduct>>(payload);
            }

            return result;
        }
    }
}
