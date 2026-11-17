using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class AdminPropertyListViewModel
{
    public List<AdminPropertyItemViewModel> Properties { get; set; } = new();
}

public class AdminPropertyItemViewModel
{
    public int Id { get; set; }

    public string Address { get; set; } = string.Empty;

    public string District { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public decimal Area { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;

    public PropertyStatus StatusValue { get; set; }

    public string RealtorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}