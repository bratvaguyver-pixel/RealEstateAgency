using System.ComponentModel.DataAnnotations;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.Models.Profiles;

namespace RealEstateAgency.Models;

public class Contract
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContractNumber { get; set; } = string.Empty;

    [Required]
    public int ClientProfileId { get; set; }

    [Required]
    public int RealtorProfileId { get; set; }

    [Required]
    public int PropertyId { get; set; }

    [Required]
    public DealType DealType { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal PropertyPrice { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal CommissionPercent { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal CommissionAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SignedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? TerminatedAt { get; set; }

    [MaxLength(3000)]
    public string? Notes { get; set; }

    public ClientProfile Client { get; set; } = null!;

    public RealtorProfile Realtor { get; set; } = null!;

    public Property Property { get; set; } = null!;
}