using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models.DTOs
{
    public class ServiceRequestDto
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

    public class CreateServiceRequestDto
    {
        [Required]
        public Guid ContractId { get; set; }

        [Required]
        public ServiceType Type { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? EstimatedCost { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [StringLength(500)]
        public string? PickupLocation { get; set; }

        [StringLength(500)]
        public string? DeliveryLocation { get; set; }
    }

    public class UpdateServiceRequestStatusDto
    {
        [Required]
        public RequestStatus Status { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ActualCost { get; set; }

        public DateTime? CompletionDate { get; set; }
    }

    public class ServiceRequestFilterDto
    {
        public Guid? ContractId { get; set; }
        public ServiceType? Type { get; set; }
        public RequestStatus? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? SortBy { get; set; } = "RequestDate";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}