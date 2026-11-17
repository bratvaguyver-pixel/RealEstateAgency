
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    // Личный кабинет клиента.
    [Authorize(Roles = UserRoles.Client)]
    public async Task<IActionResult> Client()
    {
        var userId = _userManager.GetUserId(User);

        var profile = await _dbContext.ClientProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        int requestsCount = 0;
        int contractsCount = 0;

        if (profile != null)
        {
            requestsCount = await _dbContext.Requests
                .CountAsync(request =>
                    request.ClientProfileId == profile.Id);

            contractsCount = await _dbContext.Contracts
                .CountAsync(contract =>
                    contract.ClientProfileId == profile.Id);
        }

        var model = new DashboardViewModel
        {
            Title = "Личный кабинет клиента",
            Description = "Здесь представлена основная информация о вашей работе с агентством.",
            Metrics = new List<DashboardMetricViewModel>
            {
                new()
                {
                    Label = "Мои заявки",
                    Value = requestsCount,
                    Description = "Количество созданных заявок"
                },
                new()
                {
                    Label = "Мои договоры",
                    Value = contractsCount,
                    Description = "Количество заключённых договоров"
                }
            }
        };

        return View(model);
    }

    // Личный кабинет риелтора.
    [Authorize(Roles = UserRoles.Realtor)]
    public async Task<IActionResult> Realtor()
    {
        var userId = _userManager.GetUserId(User);

        var profile = await _dbContext.RealtorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        int propertiesCount = 0;
        int requestsCount = 0;
        int completedDealsCount = 0;

        if (profile != null)
        {
            propertiesCount = await _dbContext.Properties
                .CountAsync(property =>
                    property.RealtorProfileId == profile.Id);

            requestsCount = await _dbContext.Requests
                .CountAsync(request =>
                    request.RealtorProfileId == profile.Id);

            completedDealsCount = await _dbContext.Contracts
                .CountAsync(contract =>
                    contract.RealtorProfileId == profile.Id &&
                    contract.Status == ContractStatus.Completed);
        }

        var model = new DashboardViewModel
        {
            Title = "Личный кабинет риелтора",
            Description = "Основная статистика по вашим объектам, заявкам и сделкам.",
            Metrics = new List<DashboardMetricViewModel>
            {
                new()
                {
                    Label = "Мои объекты",
                    Value = propertiesCount,
                    Description = "Объекты недвижимости в работе"
                },
                new()
                {
                    Label = "Мои заявки",
                    Value = requestsCount,
                    Description = "Назначенные вам заявки"
                },
                new()
                {
                    Label = "Завершённые сделки",
                    Value = completedDealsCount,
                    Description = "Количество завершённых договоров"
                }
            }
        };

        return View(model);
    }

    // Панель администратора.
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Admin()
    {
        int usersCount = await _dbContext.Users.CountAsync();

        int propertiesCount = await _dbContext.Properties.CountAsync();

        int requestsCount = await _dbContext.Requests.CountAsync();

        int contractsCount = await _dbContext.Contracts.CountAsync();

        var model = new DashboardViewModel
        {
            Title = "Панель администратора",
            Description = "Общая статистика информационной системы агентства недвижимости.",
            Metrics = new List<DashboardMetricViewModel>
            {
                new()
                {
                    Label = "Пользователи",
                    Value = usersCount,
                    Description = "Зарегистрированные пользователи"
                },
                new()
                {
                    Label = "Объекты недвижимости",
                    Value = propertiesCount,
                    Description = "Всего объектов в системе"
                },
                new()
                {
                    Label = "Заявки",
                    Value = requestsCount,
                    Description = "Всего созданных заявок"
                },
                new()
                {
                    Label = "Договоры",
                    Value = contractsCount,
                    Description = "Всего оформленных договоров"
                }
            }
        };

        return View(model);
    }
}
