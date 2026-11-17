
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class MySelectionsViewModel
{
    public List<MySelectionRequestViewModel> Requests { get; set; } = new();
}

public class MySelectionRequestViewModel
{
    public int RequestId { get; set; }

    public DealType DealType { get; set; }

    public RequestStatus RequestStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<MySelectionPropertyViewModel> Properties { get; set; } = new();
}

public class MySelectionPropertyViewModel
{
    public int SelectionId { get; set; }

    public int PropertyId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public PropertyType Type { get; set; }

    public decimal Area { get; set; }

    public int? RoomCount { get; set; }

    public decimal Price { get; set; }

    public PropertyStatus Status { get; set; }

    public string? MainPhotoPath { get; set; }

    public DateTime AddedAt { get; set; }

    public bool IsViewed { get; set; }

    public string? ClientComment { get; set; }
}
