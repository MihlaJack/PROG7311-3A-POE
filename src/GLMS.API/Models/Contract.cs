using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class Contract
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [Required]
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal ContractValue { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        public ContractType Type { get; set; } = ContractType.Freight;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? ServiceLevelAgreement { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }

    public enum ContractStatus
    {
        Draft = 0,
        Active = 1,
        Expired = 2,
        OnHold = 3,
        Cancelled = 4
    }

    public enum ContractType
    {
        Freight = 0,
        Warehousing = 1,
        ExpressDelivery = 2
    }
}