
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Enums;

public enum DealType
{
    [Display(Name = "Купля-продажа")]
    PurchaseSale = 1,

    [Display(Name = "Аренда")]
    Rent = 2
}
