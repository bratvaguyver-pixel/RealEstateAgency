
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;
using RequestEntity = RealEstateAgency.Models.Request;

namespace RealEstateAgency.Controllers;

[Authorize]
public class RequestsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public RequestsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize(Roles = UserRoles.Client)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new RequestCreateViewModel());
    }

    [Authorize(Roles = UserRoles.Client)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RequestCreateViewModel model)
    {
        if (model.DealType.HasValue &&
            !Enum.IsDefined(typeof(DealType), model.DealType.Value))
        {
            ModelState.AddModelError(
                nameof(model.DealType),
                "Выбран некорректный тип сделки.");
        }

        if (model.PropertyType.HasValue &&
            !Enum.IsDefined(typeof(PropertyType), model.PropertyType.Value))
        {
            ModelState.AddModelError(
                nameof(model.PropertyType),
                "Выбран некорректный тип недвижимости.");
        }

        if (model.MinPrice.HasValue &&
            model.MaxPrice.HasValue &&
            model.MinPrice > model.MaxPrice)
        {
            ModelState.AddModelError(
                nameof(model.MaxPrice),
                "Максимальный бюджет не может быть меньше минимального.");
        }

        if (model.MinArea.HasValue &&
            model.MaxArea.HasValue &&
            model.MinArea > model.MaxArea)
        {
            ModelState.AddModelError(
                nameof(model.MaxArea),
                "Максимальная площадь не может быть меньше минимальной.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var client = await _dbContext.ClientProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (client == null)
        {
            return Forbid();
        }

        // Назначаем активного риелтора с наименьшим количеством
        // новых заявок и заявок, находящихся в работе.
        var realtor = await _dbContext.RealtorProfiles
            .Where(profile => profile.Status == RealtorStatus.Active)
            .OrderBy(profile => _dbContext.Requests.Count(request =>
                request.RealtorProfileId == profile.Id &&
                (request.Status == RequestStatus.New ||
                 request.Status == RequestStatus.InProgress)))
            .ThenBy(profile => profile.Id)
            .FirstOrDefaultAsync();

        var request = new RequestEntity
        {
            ClientProfileId = client.Id,
            RealtorProfileId = realtor?.Id,
            DealType = model.DealType!.Value,
            PropertyType = model.PropertyType,
            PreferredDistrict = model.PreferredDistrict?.Trim(),
            MinPrice = model.MinPrice,
            MaxPrice = model.MaxPrice,
            MinArea = model.MinArea,
            MaxArea = model.MaxArea,
            RoomCount = model.RoomCount,
            Description = model.Description?.Trim(),
            Status = RequestStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Requests.Add(request);

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = realtor == null
            ? "Заявка создана. Сейчас нет активных риелторов, заявка ожидает назначения."
            : "Заявка успешно создана и назначена риелтору.";

        return RedirectToAction(nameof(MyRequests));
    }

    [Authorize(Roles = UserRoles.Client)]
    [HttpGet]
    public async Task<IActionResult> MyRequests()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var client = await _dbContext.ClientProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (client == null)
        {
            return Forbid();
        }

        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Where(request => request.ClientProfileId == client.Id)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new RequestListItemViewModel
            {
                Id = request.Id,
                DealType = request.DealType,
                PropertyType = request.PropertyType,
                PreferredDistrict = request.PreferredDistrict,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                MinArea = request.MinArea,
                MaxArea = request.MaxArea,
                RoomCount = request.RoomCount,
                Description = request.Description,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                RealtorName = request.Realtor == null
                    ? null
                    : request.Realtor.User.FirstName + " " +
                      request.Realtor.User.LastName
            })
            .ToListAsync();

        return View(new RequestListViewModel
        {
            Requests = requests
        });
    }

    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public async Task<IActionResult> Assigned()
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var realtor = await _dbContext.RealtorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        var requests = await _dbContext.Requests
            .AsNoTracking()
            .Where(request => request.RealtorProfileId == realtor.Id)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new RequestListItemViewModel
            {
                Id = request.Id,
                DealType = request.DealType,
                PropertyType = request.PropertyType,
                PreferredDistrict = request.PreferredDistrict,
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                MinArea = request.MinArea,
                MaxArea = request.MaxArea,
                RoomCount = request.RoomCount,
                Description = request.Description,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                ClientName = request.Client.User.FirstName + " " +
                             request.Client.User.LastName
            })
            .ToListAsync();

        return View(new RequestListViewModel
        {
            Requests = requests
        });
    }

    [Authorize(Roles = UserRoles.Realtor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        RequestStatus status)
    {
        if (!Enum.IsDefined(typeof(RequestStatus), status))
        {
            return BadRequest();
        }

        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var realtor = await _dbContext.RealtorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        // Риелтор может изменить только свою заявку.
        var request = await _dbContext.Requests
            .FirstOrDefaultAsync(item =>
                item.Id == id &&
                item.RealtorProfileId == realtor.Id);

        if (request == null)
        {
            return NotFound();
        }

        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Статус заявки успешно обновлён.";

        return RedirectToAction(nameof(Assigned));
    }
}
