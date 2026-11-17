
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class PropertyDetailsViewModel
{
    public int Id { get; set; }

    public string Address { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public PropertyType Type { get; set; }

    public decimal Area { get; set; }

    public int? RoomCount { get; set; }

    public int? Floor { get; set; }

    public decimal Price { get; set; }

    public PropertyStatus Status { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public string RealtorName { get; set; } = string.Empty;

    public List<string> PhotoPaths { get; set; } = new();
}
