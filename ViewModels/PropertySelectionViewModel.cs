
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class PropertySelectionViewModel
{
    public int RequestId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public DealType DealType { get; set; }

    public PropertyType? PropertyType { get; set; }

    public string? PreferredDistrict { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public decimal? MinArea { get; set; }

    public decimal? MaxArea { get; set; }

    public int? RoomCount { get; set; }

    public List<SelectionPropertyItemViewModel> Properties { get; set; } = new();
}

public class SelectionPropertyItemViewModel
{
    public int Id { get; set; }

    public string Address { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public PropertyType Type { get; set; }

    public decimal Area { get; set; }

    public int? RoomCount { get; set; }

    public decimal Price { get; set; }

    public string? MainPhotoPath { get; set; }
}
