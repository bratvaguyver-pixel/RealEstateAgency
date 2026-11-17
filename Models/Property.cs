using System.ComponentModel.DataAnnotations;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.Models.Profiles;

namespace RealEstateAgency.Models;

public class Property
{
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string District { get; set; } = string.Empty;

    [Required]
    public PropertyType Type { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Area { get; set; }

    [Range(0, 1000)]
    public int? RoomCount { get; set; }

    [Range(0, 200)]
    public int? Floor { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Price { get; set; }

    public PropertyStatus Status { get; set; } =
        PropertyStatus.PendingModeration;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(3000)]
    public string? Description { get; set; }

    [Required]
    public int RealtorProfileId { get; set; }

    public RealtorProfile Realtor { get; set; } = null!;

    public ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();

    public ICollection<Selection> Selections { get; set; } = new List<Selection>();

    public Contract? Contract { get; set; }
}