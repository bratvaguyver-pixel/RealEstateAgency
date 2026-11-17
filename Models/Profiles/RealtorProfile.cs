
using System.ComponentModel.DataAnnotations;
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.Models.Profiles;

public class RealtorProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Range(0, 60)]
    public int WorkExperience { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal CommissionPercent { get; set; } = 3;

    public RealtorStatus Status { get; set; } = RealtorStatus.Active;

    public int CompletedDealsCount { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ICollection<Property> Properties { get; set; } = new List<Property>();

    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
