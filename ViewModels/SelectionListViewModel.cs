
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class SelectionListViewModel
{
    public List<SelectionListItemViewModel> Selections { get; set; } = new();
}

public class SelectionListItemViewModel
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    public int PropertyId { get; set; }

    public string PropertyAddress { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public decimal Area { get; set; }

    public int? RoomCount { get; set; }

    public decimal Price { get; set; }

    public string? MainPhoto { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsViewed { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string RealtorName { get; set; } = string.Empty;

    public DealType DealType { get; set; }
}
