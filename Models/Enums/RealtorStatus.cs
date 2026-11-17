
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Enums;

public enum RealtorStatus
{
    [Display(Name = "Работает")]
    Active = 1,

    [Display(Name = "В отпуске")]
    OnVacation = 2,

    [Display(Name = "Уволен")]
    Dismissed = 3
}
