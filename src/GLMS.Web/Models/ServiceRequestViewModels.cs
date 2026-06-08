using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models
{
    public class ServiceRequestViewModel
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? PickupLocation { get; set; }
        public string? DeliveryLocation { get; set; }
    }

    public class CreateServiceRequestViewModel
    {
        [Required]
        [Display(Name = "Contract")]
        public Guid ContractId { get; set; }

        [Required]
        [Display(Name = "Service Type")]
        public string Type { get; set; } = "Freight";

        [StringLength(2000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Estimated Cost")]
        public decimal? EstimatedCost { get; set; }

        [Display(Name = "Scheduled Date")]
        [DataType(DataType.Date)]
        public DateTime? ScheduledDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Pickup Location")]
        public string? PickupLocation { get; set; }

        [StringLength(500)]
        [Display(Name = "Delivery Location")]
        public string? DeliveryLocation { get; set; }
    }

    public class PagedServiceRequestsViewModel
    {
        public List<ServiceRequestViewModel> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}