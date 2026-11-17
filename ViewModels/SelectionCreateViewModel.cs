using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RealEstateAgency.ViewModels;

public class SelectionCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Выберите заявку клиента")]
    [Display(Name = "Заявка клиента")]
    public int RequestId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Выберите объект недвижимости")]
    [Display(Name = "Объект недвижимости")]
    public int PropertyId { get; set; }

    public List<SelectListItem> Requests { get; set; } = new();

    public List<SelectListItem> Properties { get; set; } = new();
}