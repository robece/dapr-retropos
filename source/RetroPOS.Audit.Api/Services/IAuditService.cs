using RetroPOS.Audit.Api.Models;
using System.Threading.Tasks;

namespace RetroPOS.Audit.Api.Services
{
    public interface IAuditService
    {
        Task<bool> GenerateAuditFile(RegistrationRequest request);
    }
}