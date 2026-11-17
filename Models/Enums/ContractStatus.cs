
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Enums;

public enum ContractStatus
{
    [Display(Name = "Черновик")]
    Draft = 1,

    [Display(Name = "На проверке")]
    UnderReview = 2,

    [Display(Name = "Подписан")]
    Signed = 3,

    [Display(Name = "Исполняется")]
    InProgress = 4,

    [Display(Name = "Завершён")]
    Completed = 5,

    [Display(Name = "Расторгнут")]
    Terminated = 6
}
