using AutoMapper;
using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly GLMSDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceRequestService> _logger;
        private readonly IContractService _contractService;

        public ServiceRequestService(GLMSDbContext context, IMapper mapper, ILogger<ServiceRequestService> logger, IContractService contractService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _contractService = contractService;
        }

        public async Task<PagedResponse<ServiceRequestDto>> GetServiceRequestsAsync(ServiceRequestFilterDto filter)
        {
            var query = _context.ServiceRequests
                .Include(sr => sr.Contract)
                .AsNoTracking()
                .AsQueryable();

            if (filter.ContractId.HasValue)
                query = query.Where(sr => sr.ContractId == filter.ContractId.Value);

            if (filter.Type.HasValue)
                query = query.Where(sr => sr.Type == filter.Type.Value);

            if (filter.Status.HasValue)
                query = query.Where(sr => sr.Status == filter.Status.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(sr => sr.RequestDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(sr => sr.RequestDate <= filter.DateTo.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "requestnumber" => filter.SortDescending ? query.OrderByDescending(sr => sr.RequestNumber) : query.OrderBy(sr => sr.RequestNumber),
                "status" => filter.SortDescending ? query.OrderByDescending(sr => sr.Status) : query.OrderBy(sr => sr.Status),
                "type" => filter.SortDescending ? query.OrderByDescending(sr => sr.Type) : query.OrderBy(sr => sr.Type),
                _ => filter.SortDescending ? query.OrderByDescending(sr => sr.RequestDate) : query.OrderBy(sr => sr.RequestDate)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<ServiceRequestDto>>(items);

            return new PagedResponse<ServiceRequestDto>
            {
                Items = dtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }

        public async Task<ServiceRequestDto?> GetServiceRequestByIdAsync(Guid id)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .AsNoTracking()
                .FirstOrDefaultAsync(sr => sr.Id == id);

            return request == null ? null : _mapper.Map<ServiceRequestDto>(request);
        }

        public async Task<ApiResponse<ServiceRequestDto>> CreateServiceRequestAsync(CreateServiceRequestDto dto, string createdBy)
        {
            // Validate contract exists and is active
            var contract = await _context.Contracts.FindAsync(dto.ContractId);
            if (contract == null)
            {
                return ApiResponse<ServiceRequestDto>.ErrorResponse(
                    "Contract not found",
                    statusCode: 404);
            }

            if (contract.Status != ContractStatus.Active)
            {
                return ApiResponse<ServiceRequestDto>.ErrorResponse(
                    "Invalid contract status",
                    new List<string> { $"Cannot create service request for contract with status '{contract.Status}'. Contract must be Active." });
            }

            // Check if contract is expiring soon
            if (contract.EndDate < DateTime.UtcNow.AddDays(1))
            {
                return ApiResponse<ServiceRequestDto>.ErrorResponse(
                    "Contract expiring soon",
                    new List<string> { "Contract expires within 24 hours. Cannot raise new service requests." });
            }

            // Generate request number
            var requestCount = await _context.ServiceRequests.CountAsync() + 1;
            var requestNumber = $"SR-{DateTime.UtcNow:yyyy}-{requestCount:D5}";

            var serviceRequest = _mapper.Map<ServiceRequest>(dto);
            serviceRequest.RequestNumber = requestNumber;
            serviceRequest.Status = RequestStatus.Pending;

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Service request {RequestNumber} created for contract {ContractId} by {CreatedBy}",
                requestNumber, dto.ContractId, createdBy);

            var result = _mapper.Map<ServiceRequestDto>(serviceRequest);
            return ApiResponse<ServiceRequestDto>.SuccessResponse(result, "Service request created successfully");
        }

        public async Task<ApiResponse<ServiceRequestDto>> UpdateServiceRequestStatusAsync(Guid id, UpdateServiceRequestStatusDto dto, string updatedBy)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (request == null)
            {
                return ApiResponse<ServiceRequestDto>.ErrorResponse(
                    "Service request not found",
                    statusCode: 404);
            }

            // Validate status transitions
            var validTransition = (request.Status, dto.Status) switch
            {
                (RequestStatus.Pending, RequestStatus.Approved) => true,
                (RequestStatus.Pending, RequestStatus.Cancelled) => true,
                (RequestStatus.Approved, RequestStatus.InProgress) => true,
                (RequestStatus.Approved, RequestStatus.Cancelled) => true,
                (RequestStatus.InProgress, RequestStatus.Completed) => true,
                (RequestStatus.InProgress, RequestStatus.Cancelled) => true,
                (RequestStatus.Completed, _) => false,
                (RequestStatus.Cancelled, _) => false,
                _ => false
            };

            if (!validTransition)
            {
                return ApiResponse<ServiceRequestDto>.ErrorResponse(
                    "Invalid status transition",
                    new List<string> { $"Cannot transition from '{request.Status}' to '{dto.Status}'." });
            }

            request.Status = dto.Status;

            if (dto.ActualCost.HasValue)
                request.ActualCost = dto.ActualCost.Value;

            if (dto.CompletionDate.HasValue)
                request.CompletionDate = dto.CompletionDate.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Service request {RequestId} status changed to {Status} by {UpdatedBy}",
                id, dto.Status, updatedBy);

            var result = _mapper.Map<ServiceRequestDto>(request);
            return ApiResponse<ServiceRequestDto>.SuccessResponse(result, $"Service request status updated to {dto.Status}");
        }

        public async Task<ApiResponse<bool>> DeleteServiceRequestAsync(Guid id)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Service request not found",
                    statusCode: 404);
            }

            // Only allow deletion of pending requests
            if (request.Status != RequestStatus.Pending)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Cannot delete service request",
                    new List<string> { "Only Pending service requests can be deleted." });
            }

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service request {RequestId} deleted", id);
            return ApiResponse<bool>.SuccessResponse(true, "Service request deleted successfully");
        }

        public async Task<bool> ServiceRequestExistsAsync(Guid id)
        {
            return await _context.ServiceRequests.AnyAsync(sr => sr.Id == id);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetPendingServiceRequestsAsync()
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .Where(sr => sr.Status == RequestStatus.Pending)
                .OrderBy(sr => sr.RequestDate)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }
    }
}