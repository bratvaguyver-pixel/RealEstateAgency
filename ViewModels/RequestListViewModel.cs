
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class RequestListViewModel
{
    public List<RequestListItemViewModel> Requests { get; set; } = new();
}

public class RequestListItemViewModel
{
    public int Id { get; set; }

    public DealType DealType { get; set; }

    public PropertyType? PropertyType { get; set; }

    public string? PreferredDistrict { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public decimal? MinArea { get; set; }

    public decimal? MaxArea { get; set; }

    public int? RoomCount { get; set; }

    public string? Description { get; set; }

    public RequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? RealtorName { get; set; }

    public string? ClientName { get; set; }
}
