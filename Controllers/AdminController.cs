using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

[Authorize(Roles = UserRoles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Список всех пользователей.
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var currentUserId = _userManager.GetUserId(User);

        var users = await _userManager.Users
            .OrderByDescending(u => u.RegistrationDate)
            .ToListAsync();

        var model = new AdminUserListViewModel();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Users.Add(new AdminUserItemViewModel
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,

                // Отображаем роли на русском языке.
                Roles = roles.Count > 0
                    ? string.Join(", ", roles.Select(GetRoleDisplayName))
                    : "Не назначена",

                RegistrationDate = user.RegistrationDate,
                IsActive = user.IsActive,

                CanChangeStatus =
                    user.Id != currentUserId &&
                    !roles.Contains(UserRoles.Admin)
            });
        }

        return View(model);
    }

    // Русские названия ролей.
    private static string GetRoleDisplayName(string role)
    {
        return role switch
        {
            UserRoles.Client => "Клиент",
            UserRoles.Realtor => "Риелтор",
            UserRoles.Admin => "Администратор",
            _ => role
        };
    }

    // Блокировка и разблокировка пользователя.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var currentUserId = _userManager.GetUserId(User);

        if (id == currentUserId)
        {
            TempData["Error"] =
                "Нельзя заблокировать собственную учётную запись.";

            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains(UserRoles.Admin))
        {
            TempData["Error"] =
                "Нельзя заблокировать учётную запись администратора.";

            return RedirectToAction(nameof(Users));
        }

        user.IsActive = !user.IsActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            TempData["Error"] =
                "Не удалось изменить состояние учётной записи.";

            return RedirectToAction(nameof(Users));
        }

        await _userManager.UpdateSecurityStampAsync(user);

        TempData["SuccessMessage"] = user.IsActive
            ? $"Учётная запись {user.Email} разблокирована."
            : $"Учётная запись {user.Email} заблокирована.";

        return RedirectToAction(nameof(Users));
    }

    // Все объекты недвижимости агентства, включая ожидающие модерации.
    [HttpGet]
    public async Task<IActionResult> Properties()
    {
        var properties = await _context.Properties
            .Include(p => p.Realtor)
                .ThenInclude(r => r.User)
            .OrderBy(p => p.Status == PropertyStatus.PendingModeration ? 0 : 1)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        var model = new AdminPropertyListViewModel
        {
            Properties = properties.Select(p => new AdminPropertyItemViewModel
            {
                Id = p.Id,
                Address = p.Address,
                District = p.District,
                PropertyType = p.Type.GetDisplayName(),
                Area = p.Area,
                Price = p.Price,
                Status = p.Status.GetDisplayName(),
                StatusValue = p.Status,
                RealtorName =
                    $"{p.Realtor.User.FirstName} {p.Realtor.User.LastName}",
                CreatedAt = p.CreatedAt
            }).ToList()
        };

        return View(model);
    }

    // Одобрение публикации объекта.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveProperty(int id)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            return NotFound();
        }

        if (property.Status != PropertyStatus.PendingModeration)
        {
            TempData["Error"] =
                "Этот объект уже обработан или не ожидает модерации.";

            return RedirectToAction(nameof(Properties));
        }

        property.Status = PropertyStatus.Available;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Объект по адресу «{property.Address}» опубликован в каталоге.";

        return RedirectToAction(nameof(Properties));
    }

    // Отклонение публикации объекта.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectProperty(int id)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            return NotFound();
        }

        if (property.Status != PropertyStatus.PendingModeration)
        {
            TempData["Error"] =
                "Этот объект уже обработан или не ожидает модерации.";

            return RedirectToAction(nameof(Properties));
        }

        property.Status = PropertyStatus.Rejected;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Публикация объекта по адресу «{property.Address}» отклонена.";

        return RedirectToAction(nameof(Properties));
    }

    // Все заявки клиентов.
    [HttpGet]
    public async Task<IActionResult> Requests()
    {
        var requests = await _context.Requests
            .Include(r => r.Client)
                .ThenInclude(c => c.User)
            .Include(r => r.Realtor)
                .ThenInclude(r => r!.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var model = new AdminRequestListViewModel
        {
            Requests = requests.Select(r => new AdminRequestItemViewModel
            {
                Id = r.Id,

                ClientName =
                    $"{r.Client.User.FirstName} {r.Client.User.LastName}",

                RealtorName = r.Realtor == null
                    ? "Не назначен"
                    : $"{r.Realtor.User.FirstName} {r.Realtor.User.LastName}",

                DealType = r.DealType.GetDisplayName(),

                PropertyType = r.PropertyType.HasValue
                    ? r.PropertyType.Value.GetDisplayName()
                    : "Любой",

                PreferredDistrict =
                    string.IsNullOrWhiteSpace(r.PreferredDistrict)
                        ? "Не указан"
                        : r.PreferredDistrict,

                Status = r.Status.GetDisplayName(),
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return View(model);
    }

    // Все договоры агентства.
    [HttpGet]
    public async Task<IActionResult> Contracts()
    {
        var contracts = await _context.Contracts
            .Include(c => c.Client)
                .ThenInclude(c => c.User)
            .Include(c => c.Realtor)
                .ThenInclude(r => r.User)
            .Include(c => c.Property)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var model = new ContractListViewModel
        {
            Contracts = contracts.Select(c => new ContractListItemViewModel
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,

                ClientName =
                    $"{c.Client.User.FirstName} {c.Client.User.LastName}",

                RealtorName =
                    $"{c.Realtor.User.FirstName} {c.Realtor.User.LastName}",

                PropertyAddress = c.Property.Address,
                PropertyPrice = c.PropertyPrice,
                CommissionPercent = c.CommissionPercent,
                CommissionAmount = c.CommissionAmount,
                DealType = c.DealType,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList()
        };

        return View(model);
    }
}