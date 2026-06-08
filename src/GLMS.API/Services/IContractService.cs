using GLMS.API.Models;
using GLMS.API.Models.DTOs;

namespace GLMS.API.Services
{
    public interface IContractService
    {
        Task<PagedResponse<ContractDto>> GetContractsAsync(ContractFilterDto filter);
        Task<ContractDto?> GetContractByIdAsync(Guid id);
        Task<ContractDto?> GetContractByNumberAsync(string contractNumber);
        Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto dto, string createdBy);
        Task<ApiResponse<ContractDto>> UpdateContractAsync(Guid id, UpdateContractDto dto, string updatedBy);
        Task<ApiResponse<ContractDto>> UpdateContractStatusAsync(Guid id, UpdateContractStatusDto dto, string updatedBy);
        Task<ApiResponse<bool>> DeleteContractAsync(Guid id);
        Task<bool> ContractExistsAsync(Guid id);
        Task<bool> HasActiveContractsForClientAsync(string clientEmail);
        Task<IEnumerable<ContractDto>> GetExpiringContractsAsync(int daysThreshold);
        Task<ApiResponse<ContractDto>> ActivateContractAsync(Guid id, string updatedBy);
        Task<ApiResponse<ContractDto>> ExpireContractAsync(Guid id, string updatedBy);
    }
}