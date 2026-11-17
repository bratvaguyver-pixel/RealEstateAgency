namespace RealEstateAgency.ViewModels;

public class AdminRequestListViewModel
{
    public List<AdminRequestItemViewModel> Requests { get; set; } = new();
}

public class AdminRequestItemViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string RealtorName { get; set; } = string.Empty;

    public string DealType { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string PreferredDistrict { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}