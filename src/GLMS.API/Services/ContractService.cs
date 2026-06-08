using AutoMapper;
using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Services
{
    public class ContractService : IContractService
    {
        private readonly GLMSDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ContractService> _logger;
        private readonly ICacheService _cache;

        public ContractService(GLMSDbContext context, IMapper mapper, ILogger<ContractService> logger, ICacheService cache)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public async Task<PagedResponse<ContractDto>> GetContractsAsync(ContractFilterDto filter)
        {
            var cacheKey = $"contracts_{filter.GetHashCode()}";
            var cached = await _cache.GetAsync<PagedResponse<ContractDto>>(cacheKey);
            if (cached != null) return cached;

            var query = _context.Contracts
                .Include(c => c.ServiceRequests)
                .AsNoTracking()
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var search = filter.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.ContractNumber.ToLower().Contains(search) ||
                    c.ClientName.ToLower().Contains(search) ||
                    c.ClientEmail.ToLower().Contains(search));
            }

            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);

            if (filter.Type.HasValue)
                query = query.Where(c => c.Type == filter.Type.Value);

            if (filter.StartDateFrom.HasValue)
                query = query.Where(c => c.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(c => c.StartDate <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                query = query.Where(c => c.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                query = query.Where(c => c.EndDate <= filter.EndDateTo.Value);

            if (filter.MinValue.HasValue)
                query = query.Where(c => c.ContractValue >= filter.MinValue.Value);

            if (filter.MaxValue.HasValue)
                query = query.Where(c => c.ContractValue <= filter.MaxValue.Value);

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "contractnumber" => filter.SortDescending ? query.OrderByDescending(c => c.ContractNumber) : query.OrderBy(c => c.ContractNumber),
                "clientname" => filter.SortDescending ? query.OrderByDescending(c => c.ClientName) : query.OrderBy(c => c.ClientName),
                "contractvalue" => filter.SortDescending ? query.OrderByDescending(c => c.ContractValue) : query.OrderBy(c => c.ContractValue),
                "startdate" => filter.SortDescending ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate),
                "enddate" => filter.SortDescending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate),
                "status" => filter.SortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
                _ => filter.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<ContractDto>>(items);
            foreach (var dto in dtos)
            {
                dto.ServiceRequestCount = items.First(c => c.Id == dto.Id).ServiceRequests.Count;
            }

            var response = new PagedResponse<ContractDto>
            {
                Items = dtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            };

            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<ContractDto?> GetContractByIdAsync(Guid id)
        {
            var cacheKey = $"contract_{id}";
            var cached = await _cache.GetAsync<ContractDto>(cacheKey);
            if (cached != null) return cached;

            var contract = await _context.Contracts
                .Include(c => c.ServiceRequests)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null) return null;

            var dto = _mapper.Map<ContractDto>(contract);
            dto.ServiceRequestCount = contract.ServiceRequests.Count;

            await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
            return dto;
        }

        public async Task<ContractDto?> GetContractByNumberAsync(string contractNumber)
        {
            var contract = await _context.Contracts
                .Include(c => c.ServiceRequests)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);

            return contract == null ? null : _mapper.Map<ContractDto>(contract);
        }

        public async Task<ApiResponse<ContractDto>> CreateContractAsync(CreateContractDto dto, string createdBy)
        {
            // Validate contract number uniqueness
            if (await _context.Contracts.AnyAsync(c => c.ContractNumber == dto.ContractNumber))
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Contract number already exists",
                    new List<string> { $"Contract number '{dto.ContractNumber}' is already in use." },
                    409);
            }

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Invalid date range",
                    new List<string> { "End date must be after start date." });
            }

            // Validate contract value based on type
            var minValue = dto.Type switch
            {
                ContractType.Freight => 1000m,
                ContractType.Warehousing => 5000m,
                ContractType.ExpressDelivery => 500m,
                _ => 0m
            };

            if (dto.ContractValue < minValue)
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Invalid contract value",
                    new List<string> { $"{dto.Type} contracts must have a minimum value of {minValue:C}." });
            }

            var contract = _mapper.Map<Contract>(dto);
            contract.CreatedBy = createdBy;
            contract.Status = ContractStatus.Draft;

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract {ContractNumber} created by {CreatedBy}", dto.ContractNumber, createdBy);
            await _cache.RemoveAsync("contracts_");

            var result = _mapper.Map<ContractDto>(contract);
            return ApiResponse<ContractDto>.SuccessResponse(result, "Contract created successfully");
        }

        public async Task<ApiResponse<ContractDto>> UpdateContractAsync(Guid id, UpdateContractDto dto, string updatedBy)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Contract not found",
                    statusCode: 404);
            }

            // Only allow updates on Draft or Active contracts
            if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.Active)
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Cannot update contract",
                    new List<string> { $"Contracts with status '{contract.Status}' cannot be modified." });
            }

            // Apply updates
            if (!string.IsNullOrWhiteSpace(dto.ClientName))
                contract.ClientName = dto.ClientName;

            if (!string.IsNullOrWhiteSpace(dto.ClientEmail))
                contract.ClientEmail = dto.ClientEmail;

            if (!string.IsNullOrWhiteSpace(dto.ClientPhone))
                contract.ClientPhone = dto.ClientPhone;

            if (dto.StartDate.HasValue)
                contract.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                contract.EndDate = dto.EndDate.Value;

            if (dto.ContractValue.HasValue)
                contract.ContractValue = dto.ContractValue.Value;

            if (!string.IsNullOrWhiteSpace(dto.Currency))
                contract.Currency = dto.Currency;

            if (dto.Type.HasValue)
                contract.Type = dto.Type.Value;

            if (dto.Description != null)
                contract.Description = dto.Description;

            if (dto.ServiceLevelAgreement != null)
                contract.ServiceLevelAgreement = dto.ServiceLevelAgreement;

            contract.UpdatedAt = DateTime.UtcNow;
            contract.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract {ContractId} updated by {UpdatedBy}", id, updatedBy);
            await _cache.RemoveAsync($"contract_{id}");
            await _cache.RemoveAsync("contracts_");

            var result = _mapper.Map<ContractDto>(contract);
            return ApiResponse<ContractDto>.SuccessResponse(result, "Contract updated successfully");
        }

        public async Task<ApiResponse<ContractDto>> UpdateContractStatusAsync(Guid id, UpdateContractStatusDto dto, string updatedBy)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Contract not found",
                    statusCode: 404);
            }

            // Validate state transitions
            var validTransition = (contract.Status, dto.Status) switch
            {
                (ContractStatus.Draft, ContractStatus.Active) => true,
                (ContractStatus.Draft, ContractStatus.Cancelled) => true,
                (ContractStatus.Active, ContractStatus.Expired) => true,
                (ContractStatus.Active, ContractStatus.OnHold) => true,
                (ContractStatus.OnHold, ContractStatus.Active) => true,
                (ContractStatus.OnHold, ContractStatus.Expired) => true,
                (ContractStatus.Expired, ContractStatus.Active) => true, // Renewal
                (ContractStatus.Cancelled, _) => false,
                _ => false
            };

            if (!validTransition)
            {
                return ApiResponse<ContractDto>.ErrorResponse(
                    "Invalid status transition",
                    new List<string> { $"Cannot transition from '{contract.Status}' to '{dto.Status}'." });
            }

            // Additional validation for activation
            if (dto.Status == ContractStatus.Active && contract.Status == ContractStatus.Draft)
            {
                if (contract.StartDate > DateTime.UtcNow)
                {
                    return ApiResponse<ContractDto>.ErrorResponse(
                        "Cannot activate contract",
                        new List<string> { "Contract start date is in the future." });
                }
            }

            // Additional validation for expiration
            if (dto.Status == ContractStatus.Expired && contract.Status == ContractStatus.Active)
            {
                if (contract.EndDate > DateTime.UtcNow)
                {
                    return ApiResponse<ContractDto>.ErrorResponse(
                        "Cannot expire contract",
                        new List<string> { "Contract end date has not been reached yet." });
                }
            }

            contract.Status = dto.Status;
            contract.UpdatedAt = DateTime.UtcNow;
            contract.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Contract {ContractId} status changed from {OldStatus} to {NewStatus} by {UpdatedBy}",
                id, contract.Status, dto.Status, updatedBy);

            await _cache.RemoveAsync($"contract_{id}");
            await _cache.RemoveAsync("contracts_");

            var result = _mapper.Map<ContractDto>(contract);
            return ApiResponse<ContractDto>.SuccessResponse(result, $"Contract status updated to {dto.Status}");
        }

        public async Task<ApiResponse<bool>> DeleteContractAsync(Guid id)
        {
            var contract = await _context.Contracts
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Contract not found",
                    statusCode: 404);
            }

            // Only allow deletion of Draft contracts with no service requests
            if (contract.Status != ContractStatus.Draft)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Cannot delete contract",
                    new List<string> { "Only Draft contracts can be deleted." });
            }

            if (contract.ServiceRequests.Any())
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Cannot delete contract",
                    new List<string> { "Contract has associated service requests." });
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contract {ContractId} deleted", id);
            await _cache.RemoveAsync($"contract_{id}");
            await _cache.RemoveAsync("contracts_");

            return ApiResponse<bool>.SuccessResponse(true, "Contract deleted successfully");
        }

        public async Task<bool> ContractExistsAsync(Guid id)
        {
            return await _context.Contracts.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> HasActiveContractsForClientAsync(string clientEmail)
        {
            return await _context.Contracts
                .AnyAsync(c => c.ClientEmail == clientEmail && c.Status == ContractStatus.Active);
        }

        public async Task<IEnumerable<ContractDto>> GetExpiringContractsAsync(int daysThreshold)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(daysThreshold);
            var contracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Active && c.EndDate <= thresholdDate)
                .OrderBy(c => c.EndDate)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<ContractDto>>(contracts);
        }

        public async Task<ApiResponse<ContractDto>> ActivateContractAsync(Guid id, string updatedBy)
        {
            return await UpdateContractStatusAsync(id, new UpdateContractStatusDto { Status = ContractStatus.Active }, updatedBy);
        }

        public async Task<ApiResponse<ContractDto>> ExpireContractAsync(Guid id, string updatedBy)
        {
            return await UpdateContractStatusAsync(id, new UpdateContractStatusDto { Status = ContractStatus.Expired }, updatedBy);
        }
    }
}