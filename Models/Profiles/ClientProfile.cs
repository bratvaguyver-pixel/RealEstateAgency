
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Profiles;

public class ClientProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PassportSeries { get; set; }

    [MaxLength(50)]
    public string? PassportNumber { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? Budget { get; set; }

    [MaxLength(150)]
    public string? PreferredDistrict { get; set; }

    [MaxLength(100)]
    public string? PreferredDealType { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ICollection<Request> Requests { get; set; } = new List<Request>();

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
