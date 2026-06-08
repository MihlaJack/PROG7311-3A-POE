using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models.DTOs
{
    // GET Response DTO
    public class ContractDto
    {
        public Guid Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ServiceLevelAgreement { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ServiceRequestCount { get; set; }
    }

    // POST Request DTO
    public class CreateContractDto
    {
        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string ClientEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ClientPhone { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Contract value must be greater than 0")]
        public decimal ContractValue { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public ContractType Type { get; set; } = ContractType.Freight;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? ServiceLevelAgreement { get; set; }
    }

    // PATCH Status DTO
    public class UpdateContractStatusDto
    {
        [Required]
        public ContractStatus Status { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }

    // PATCH General DTO
    public class UpdateContractDto
    {
        [StringLength(200)]
        public string? ClientName { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? ClientEmail { get; set; }

        [StringLength(20)]
        public string? ClientPhone { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? ContractValue { get; set; }

        [StringLength(3)]
        public string? Currency { get; set; }

        public ContractType? Type { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? ServiceLevelAgreement { get; set; }
    }

    // Filter DTO
    public class ContractFilterDto
    {
        public string? SearchTerm { get; set; }
        public ContractStatus? Status { get; set; }
        public ContractType? Type { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // Paginated Response
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}