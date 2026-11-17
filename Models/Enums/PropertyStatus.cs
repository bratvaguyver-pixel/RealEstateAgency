
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Enums;

public enum PropertyStatus
{
    [Display(Name = "Доступен")]
    Available = 1,

    [Display(Name = "Забронирован")]
    Reserved = 2,

    [Display(Name = "Продан")]
    Sold = 3,

    [Display(Name = "Сдан в аренду")]
    Rented = 4
}
