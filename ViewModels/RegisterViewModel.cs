
using System.ComponentModel.DataAnnotations;

namespace RealEstateAgency.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя.")]
    [MaxLength(100, ErrorMessage = "Имя не должно превышать 100 символов.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите фамилию.")]
    [MaxLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите электронную почту.")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [MinLength(8, ErrorMessage = "Пароль должен содержать минимум 8 символов.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повторите пароль.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите тип аккаунта.")]
    [RegularExpression("^(Client|Realtor)$",
        ErrorMessage = "Выберите корректный тип аккаунта.")]
    public string Role { get; set; } = "Client";
}
