using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ContractViewModel
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
        public int ServiceRequestCount { get; set; }
    }

    public class CreateContractViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Contract Number")]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        [Display(Name = "Client Email")]
        public string ClientEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Client Phone")]
        public string ClientPhone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(12);

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than 0")]
        [Display(Name = "Contract Value")]
        public decimal ContractValue { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "USD";

        [Required]
        [Display(Name = "Contract Type")]
        public string Type { get; set; } = "Freight";

        [StringLength(2000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(1000)]
        [Display(Name = "Service Level Agreement")]
        public string? ServiceLevelAgreement { get; set; }
    }

    public class UpdateContractStatusViewModel
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Reason")]
        public string? Reason { get; set; }
    }

    public class ContractFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedContractsViewModel
    {
        public List<ContractViewModel> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}