
namespace RealEstateAgency.ViewModels;

public class DashboardViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<DashboardMetricViewModel> Metrics { get; set; } = new();
}

public class DashboardMetricViewModel
{
    public string Label { get; set; } = string.Empty;

    public int Value { get; set; }

    public string Description { get; set; } = string.Empty;
}
