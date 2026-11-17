
using System.ComponentModel.DataAnnotations;
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class RequestCreateViewModel
{
    [Required(ErrorMessage = "Выберите тип сделки.")]
    [Display(Name = "Тип сделки")]
    public DealType? DealType { get; set; }

    [Display(Name = "Тип недвижимости")]
    public PropertyType? PropertyType { get; set; }

    [MaxLength(150, ErrorMessage = "Название района слишком длинное.")]
    [Display(Name = "Желаемый район")]
    public string? PreferredDistrict { get; set; }

    [Range(0, 999999999999, ErrorMessage = "Укажите корректную минимальную цену.")]
    [Display(Name = "Бюджет от, ₽")]
    public decimal? MinPrice { get; set; }

    [Range(0, 999999999999, ErrorMessage = "Укажите корректную максимальную цену.")]
    [Display(Name = "Бюджет до, ₽")]
    public decimal? MaxPrice { get; set; }

    [Range(0, 999999999999, ErrorMessage = "Укажите корректную минимальную площадь.")]
    [Display(Name = "Площадь от, м²")]
    public decimal? MinArea { get; set; }

    [Range(0, 999999999999, ErrorMessage = "Укажите корректную максимальную площадь.")]
    [Display(Name = "Площадь до, м²")]
    public decimal? MaxArea { get; set; }

    [Range(0, 1000, ErrorMessage = "Укажите корректное количество комнат.")]
    [Display(Name = "Количество комнат")]
    public int? RoomCount { get; set; }

    [MaxLength(3000, ErrorMessage = "Описание не должно превышать 3000 символов.")]
    [Display(Name = "Дополнительные пожелания")]
    public string? Description { get; set; }
}
