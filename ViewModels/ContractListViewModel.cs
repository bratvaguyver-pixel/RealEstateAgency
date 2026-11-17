using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class ContractListViewModel
{
    public List<ContractListItemViewModel> Contracts { get; set; } = new();
}

public class ContractListItemViewModel
{
    public int Id { get; set; }

    public string ContractNumber { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string RealtorName { get; set; } = string.Empty;

    public string PropertyAddress { get; set; } = string.Empty;

    public decimal PropertyPrice { get; set; }

    public decimal CommissionPercent { get; set; }

    public decimal CommissionAmount { get; set; }

    public DealType DealType { get; set; }

    public ContractStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}