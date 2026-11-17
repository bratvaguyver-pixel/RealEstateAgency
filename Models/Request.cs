using System.ComponentModel.DataAnnotations;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.Models.Profiles;

namespace RealEstateAgency.Models;

public class Request
{
    public int Id { get; set; }

    [Required]
    public int ClientProfileId { get; set; }

    public int? RealtorProfileId { get; set; }

    [Required]
    public DealType DealType { get; set; }

    public PropertyType? PropertyType { get; set; }

    [MaxLength(150)]
    public string? PreferredDistrict { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? MinPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? MaxPrice { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? MinArea { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? MaxArea { get; set; }

    [Range(0, 1000)]
    public int? RoomCount { get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.New;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ClientProfile Client { get; set; } = null!;

    public RealtorProfile? Realtor { get; set; }

    public ICollection<Selection> Selections { get; set; } = new List<Selection>();
}