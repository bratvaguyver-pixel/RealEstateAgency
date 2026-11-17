
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

[Authorize]
public class SelectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SelectionsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Открытие формы создания подбора.
    // requestId передаётся из карточки заявки риелтора.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public async Task<IActionResult> Create(int? requestId)
    {
        var model = new SelectionCreateViewModel
        {
            RequestId = requestId ?? 0
        };

        await LoadCreateListsAsync(model, model.RequestId);

        if (requestId.HasValue &&
            !model.Requests.Any(r => r.Value == requestId.Value.ToString()))
        {
            TempData["Error"] =
                "Заявка не найдена, уже закрыта или не назначена вам.";

            model.RequestId = 0;
            await LoadCreateListsAsync(model, 0);
        }

        if (model.Requests.Count == 0)
        {
            TempData["Error"] =
                "У вас пока нет заявок, для которых можно создать подбор.";
        }
        else if (model.Properties.Count == 0)
        {
            TempData["Error"] =
                "Для выбранной заявки нет доступных объектов подходящего типа.";
        }

        return View(model);
    }

    // Сохранение подбора.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SelectionCreateViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        var realtor = await _context.RealtorProfiles
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        // Получаем только заявку текущего риелтора.
        // Закрытые и отменённые заявки использовать нельзя.
        var request = await _context.Requests
            .FirstOrDefaultAsync(r =>
                r.Id == model.RequestId &&
                r.RealtorProfileId == realtor.Id &&
                (r.Status == RequestStatus.New ||
                 r.Status == RequestStatus.InProgress));

        if (request == null)
        {
            ModelState.AddModelError(
                nameof(model.RequestId),
                "Заявка не найдена, недоступна или уже закрыта.");
        }

        // Получаем только собственный доступный объект риелтора.
        var property = await _context.Properties
            .FirstOrDefaultAsync(p =>
                p.Id == model.PropertyId &&
                p.RealtorProfileId == realtor.Id &&
                p.Status == PropertyStatus.Available);

        if (property == null)
        {
            ModelState.AddModelError(
                nameof(model.PropertyId),
                "Объект не найден, недоступен или принадлежит другому риелтору.");
        }

        // Проверяем соответствие типа недвижимости заявке.
        if (request != null &&
            property != null &&
            request.PropertyType.HasValue &&
            request.PropertyType.Value != property.Type)
        {
            ModelState.AddModelError(
                nameof(model.PropertyId),
                $"Тип объекта не соответствует заявке. " +
                $"Клиент ищет: {request.PropertyType.Value.GetDisplayName()}, " +
                $"а выбранный объект: {property.Type.GetDisplayName()}.");
        }

        // Не допускаем повторное добавление одного объекта
        // в одну и ту же заявку.
        if (request != null && property != null)
        {
            var alreadyExists = await _context.Selections
                .AnyAsync(s =>
                    s.RequestId == request.Id &&
                    s.PropertyId == property.Id);

            if (alreadyExists)
            {
                ModelState.AddModelError(
                    nameof(model.PropertyId),
                    "Этот объект уже добавлен в подбор по выбранной заявке.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadCreateListsAsync(model, model.RequestId);
            return View(model);
        }

        var selection = new Selection
        {
            RequestId = request!.Id,
            PropertyId = property!.Id,
            CreatedAt = DateTime.UtcNow,
            IsViewed = false
        };

        _context.Selections.Add(selection);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Объект успешно добавлен в подбор клиента.";

        return RedirectToAction(nameof(RealtorSelections));
    }

    // Подборы, созданные текущим риелтором.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public async Task<IActionResult> RealtorSelections()
    {
        var userId = _userManager.GetUserId(User);

        var realtor = await _context.RealtorProfiles
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        var selections = await _context.Selections
            .Where(s => s.Request.RealtorProfileId == realtor.Id)
            .Include(s => s.Property)
                .ThenInclude(p => p.Photos)
            .Include(s => s.Request)
                .ThenInclude(r => r.Client)
                    .ThenInclude(c => c.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var model = new SelectionListViewModel
        {
            Selections = selections.Select(s => new SelectionListItemViewModel
            {
                Id = s.Id,
                RequestId = s.RequestId,
                PropertyId = s.PropertyId,
                PropertyAddress = s.Property.Address,
                District = s.Property.District,
                PropertyType = s.Property.Type.GetDisplayName(),
                Area = s.Property.Area,
                RoomCount = s.Property.RoomCount,
                Price = s.Property.Price,
                MainPhoto = s.Property.Photos
                    .FirstOrDefault(p => p.IsMain)?.FilePath
                    ?? s.Property.Photos.FirstOrDefault()?.FilePath,
                CreatedAt = s.CreatedAt,
                IsViewed = s.IsViewed,
                ClientName =
                    $"{s.Request.Client.User.FirstName} {s.Request.Client.User.LastName}",
                DealType = s.Request.DealType
            }).ToList()
        };

        return View(model);
    }

    // Подборы, предложенные текущему клиенту.
    [Authorize(Roles = UserRoles.Client)]
    [HttpGet]
    public async Task<IActionResult> MySelections()
    {
        var userId = _userManager.GetUserId(User);

        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (client == null)
        {
            return Forbid();
        }

        var selections = await _context.Selections
            .Where(s => s.Request.ClientProfileId == client.Id)
            .Include(s => s.Property)
                .ThenInclude(p => p.Photos)
            .Include(s => s.Request)
                .ThenInclude(r => r.Realtor)
                    .ThenInclude(r => r!.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // Отмечаем подборы как просмотренные.
        var unreadSelections = selections
            .Where(s => !s.IsViewed)
            .ToList();

        foreach (var selection in unreadSelections)
        {
            selection.IsViewed = true;
        }

        if (unreadSelections.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        var model = new SelectionListViewModel
        {
            Selections = selections.Select(s => new SelectionListItemViewModel
            {
                Id = s.Id,
                RequestId = s.RequestId,
                PropertyId = s.PropertyId,
                PropertyAddress = s.Property.Address,
                District = s.Property.District,
                PropertyType = s.Property.Type.GetDisplayName(),
                Area = s.Property.Area,
                RoomCount = s.Property.RoomCount,
                Price = s.Property.Price,
                MainPhoto = s.Property.Photos
                    .FirstOrDefault(p => p.IsMain)?.FilePath
                    ?? s.Property.Photos.FirstOrDefault()?.FilePath,
                CreatedAt = s.CreatedAt,
                IsViewed = true,
                RealtorName = s.Request.Realtor == null
                    ? "Не назначен"
                    : $"{s.Request.Realtor.User.FirstName} {s.Request.Realtor.User.LastName}",
                DealType = s.Request.DealType
            }).ToList()
        };

        return View(model);
    }

    // Заполняет списки заявок и объектов для формы.
    // Если заявка выбрана, объекты фильтруются по её типу.
    private async Task LoadCreateListsAsync(
        SelectionCreateViewModel model,
        int selectedRequestId)
    {
        var userId = _userManager.GetUserId(User);

        var realtor = await _context.RealtorProfiles
            .FirstOrDefaultAsync(r => r.UserId == userId);

        if (realtor == null)
        {
            return;
        }

        var requests = await _context.Requests
            .Where(r =>
                r.RealtorProfileId == realtor.Id &&
                (r.Status == RequestStatus.New ||
                 r.Status == RequestStatus.InProgress))
            .Include(r => r.Client)
                .ThenInclude(c => c.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        model.Requests = requests.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = $"Заявка №{r.Id} — " +
                   $"{r.Client.User.FirstName} {r.Client.User.LastName}" +
                   (r.PropertyType.HasValue
                       ? $" ({r.PropertyType.Value.GetDisplayName()})"
                       : " (любой тип)"),
            Selected = r.Id == selectedRequestId
        }).ToList();

        var selectedRequest = requests
            .FirstOrDefault(r => r.Id == selectedRequestId);

        var propertiesQuery = _context.Properties
            .Where(p =>
                p.RealtorProfileId == realtor.Id &&
                p.Status == PropertyStatus.Available);

        // Если в заявке указан тип недвижимости,
        // показываем только объекты этого типа.
        if (selectedRequest?.PropertyType is PropertyType propertyType)
        {
            propertiesQuery = propertiesQuery
                .Where(p => p.Type == propertyType);
        }

        var properties = await propertiesQuery
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        model.Properties = properties.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.Address} — {p.Type.GetDisplayName()} — {p.Price:N0} ₽"
        }).ToList();
    }
}
