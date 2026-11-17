
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class PropertyFilterViewModel
{
    public string? District { get; set; }

    public PropertyType? Type { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public decimal? MinArea { get; set; }

    public decimal? MaxArea { get; set; }

    public PropertyStatus? Status { get; set; }

    public List<PropertyListItemViewModel> Properties { get; set; } = new();
}

public class PropertyListItemViewModel
{
    public int Id { get; set; }

    public string Address { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public PropertyType Type { get; set; }

    public decimal Area { get; set; }

    public int? RoomCount { get; set; }

    public decimal Price { get; set; }

    public PropertyStatus Status { get; set; }

    public string? MainPhotoPath { get; set; }
}
