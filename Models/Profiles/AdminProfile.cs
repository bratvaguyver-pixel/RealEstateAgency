using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Profiles;

public class AdminProfile
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public bool HasFullAccess { get; set; } = true;

    public ApplicationUser User { get; set; } = null!;
}
