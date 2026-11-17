
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Enums;

public enum PropertyType
{
    [Display(Name = "Квартира")]
    Apartment = 1,

    [Display(Name = "Дом")]
    House = 2,

    [Display(Name = "Земельный участок")]
    Land = 3,

    [Display(Name = "Коммерческая недвижимость")]
    Commercial = 4
}
