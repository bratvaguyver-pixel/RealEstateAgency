
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.Models.Enums;

public enum RequestStatus
{
    [Display(Name = "Новая")]
    New = 1,

    [Display(Name = "В работе")]
    InProgress = 2,

    [Display(Name = "Завершена")]
    Completed = 3,

    [Display(Name = "Отменена")]
    Cancelled = 4
}
