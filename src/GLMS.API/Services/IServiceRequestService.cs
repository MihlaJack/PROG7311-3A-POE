using GLMS.API.Models;
using GLMS.API.Models.DTOs;

namespace GLMS.API.Services
{
    public interface IServiceRequestService
    {
        Task<PagedResponse<ServiceRequestDto>> GetServiceRequestsAsync(ServiceRequestFilterDto filter);
        Task<ServiceRequestDto?> GetServiceRequestByIdAsync(Guid id);
        Task<ApiResponse<ServiceRequestDto>> CreateServiceRequestAsync(CreateServiceRequestDto dto, string createdBy);
        Task<ApiResponse<ServiceRequestDto>> UpdateServiceRequestStatusAsync(Guid id, UpdateServiceRequestStatusDto dto, string updatedBy);
        Task<ApiResponse<bool>> DeleteServiceRequestAsync(Guid id);
        Task<bool> ServiceRequestExistsAsync(Guid id);
        Task<IEnumerable<ServiceRequestDto>> GetPendingServiceRequestsAsync();
    }
}