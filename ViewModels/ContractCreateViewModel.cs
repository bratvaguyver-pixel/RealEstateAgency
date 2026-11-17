using System.ComponentModel.DataAnnotations;
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class ContractCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Некорректный подбор.")]
    public int SelectionId { get; set; }

    [Display(Name = "Номер договора")]
    public string ContractNumber { get; set; } = string.Empty;

    [Display(Name = "Клиент")]
    public string ClientName { get; set; } = string.Empty;

    [Display(Name = "Объект недвижимости")]
    public string PropertyAddress { get; set; } = string.Empty;

    [Display(Name = "Тип сделки")]
    public DealType DealType { get; set; }

    [Display(Name = "Стоимость недвижимости")]
    public decimal PropertyPrice { get; set; }

    [Display(Name = "Комиссия риелтора")]
    public decimal CommissionPercent { get; set; }

    [Display(Name = "Сумма комиссии")]
    public decimal CommissionAmount { get; set; }

    [StringLength(3000, ErrorMessage = "Примечание не должно превышать 3000 символов.")]
    [Display(Name = "Примечание к договору")]
    public string? Notes { get; set; }
}