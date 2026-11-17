
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateAgency.Models.Enums;

namespace RealEstateAgency.ViewModels;

public class PropertyCreateViewModel
{
    [Required(ErrorMessage = "Укажите адрес объекта.")]
    [MaxLength(300)]
    [Display(Name = "Адрес")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите район.")]
    [MaxLength(150)]
    [Display(Name = "Район")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите тип недвижимости.")]
    [Display(Name = "Тип недвижимости")]
    public PropertyType Type { get; set; }

    [Required(ErrorMessage = "Укажите площадь.")]
    [Range(typeof(decimal), "0.01", "999999999999",
        ErrorMessage = "Площадь должна быть больше нуля.")]
    [Display(Name = "Площадь, м²")]
    public decimal Area { get; set; }

    [Range(0, 1000, ErrorMessage = "Некорректное количество комнат.")]
    [Display(Name = "Количество комнат")]
    public int? RoomCount { get; set; }

    [Range(0, 200, ErrorMessage = "Некорректный этаж.")]
    [Display(Name = "Этаж")]
    public int? Floor { get; set; }

    [Required(ErrorMessage = "Укажите цену.")]
    [Range(typeof(decimal), "0.01", "999999999999",
        ErrorMessage = "Цена должна быть больше нуля.")]
    [Display(Name = "Цена")]
    public decimal Price { get; set; }

    [MaxLength(3000)]
    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Фотографии")]
    public List<IFormFile> Photos { get; set; } = new();
}
