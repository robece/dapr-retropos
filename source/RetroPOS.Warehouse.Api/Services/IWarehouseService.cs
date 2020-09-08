using RetroPOS.Warehouse.Api.Models;
using System.Threading.Tasks;

namespace RetroPOS.Warehouse.Api.Services
{
    public interface IWarehouseService
    {
        Task<bool> ProductUpdateRegistration(ProductUpdateRegistrationRequest request);
        Task<bool> ProductDeletion(ProductDeletionRequest request);
        Task<bool> NotifySubscriptors(ProductUpdateRegistrationRequest request);
        Task<WarehouseProductsResult> WarehouseProducts(WarehouseProductsRequest request);
    }
}