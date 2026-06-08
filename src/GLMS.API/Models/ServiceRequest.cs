using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class ServiceRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid ContractId { get; set; }

        [ForeignKey("ContractId")]
        public Contract Contract { get; set; } = null!;

        [Required]
        public ServiceType Type { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCost { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        [StringLength(500)]
        public string? PickupLocation { get; set; }

        [StringLength(500)]
        public string? DeliveryLocation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ServiceType
    {
        Freight = 0,
        Warehousing = 1,
        ExpressDelivery = 2
    }

    public enum RequestStatus
    {
        Pending = 0,
        Approved = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}