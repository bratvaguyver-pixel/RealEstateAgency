
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;
using SelectionEntity = RealEstateAgency.Models.Selection;

namespace RealEstateAgency.Controllers;

[Authorize]
public class SelectionsController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public SelectionsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Подбор объектов для заявки риелтором
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public async Task<IActionResult> ForRequest(int requestId)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var realtor = await _dbContext.RealtorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        var request = await _dbContext.Requests
            .AsNoTracking()
            .Include(item => item.Client)
                .ThenInclude(client => client.User)
            .FirstOrDefaultAsync(item =>
                item.Id == requestId &&
                item.RealtorProfileId == realtor.Id);

        if (request == null)
        {
            return NotFound();
        }

        var selectedPropertyIds = await _dbContext.Selections
            .Where(selection => selection.RequestId == request.Id)
            .Select(selection => selection.PropertyId)
            .ToListAsync();

        var propertiesQuery = _dbContext.Properties
            .AsNoTracking()
            .Where(property => property.Status == PropertyStatus.Available);

        if (request.PropertyType.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(property =>
                property.Type == request.PropertyType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredDistrict))
        {
            string district = request.PreferredDistrict.Trim().ToLower();

            propertiesQuery = propertiesQuery.Where(property =>
                property.District.ToLower().Contains(district));
        }

        if (request.MinPrice.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(property =>
                property.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(property =>
                property.Price <= request.MaxPrice.Value);
        }

        if (request.MinArea.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(property =>
                property.Area >= request.MinArea.Value);
        }

        if (request.MaxArea.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(property =>
                property.Area <= request.MaxArea.Value);
        }

        if (request.RoomCount.HasValue)
        {
            propertiesQuery = propertiesQuery.Where(property =>
                property.RoomCount == request.RoomCount.Value);
        }

        var properties = await propertiesQuery
            .OrderBy(property => property.Price)
            .Select(property => new SelectionPropertyItemViewModel
            {
                Id = property.Id,
                Address = property.Address,
                District = property.District,
                Type = property.Type,
                Area = property.Area,
                RoomCount = property.RoomCount,
                Price = property.Price,
                MainPhotoPath = property.Photos
                    .Where(photo => photo.IsMain)
                    .Select(photo => photo.FilePath)
                    .FirstOrDefault()
            })
            .ToListAsync();

        properties = properties
            .Where(property => !selectedPropertyIds.Contains(property.Id))
            .ToList();

        var model = new PropertySelectionViewModel
        {
            RequestId = request.Id,
            ClientName =
                $"{request.Client.User.FirstName} {request.Client.User.LastName}",
            DealType = request.DealType,
            PropertyType = request.PropertyType,
            PreferredDistrict = request.PreferredDistrict,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            MinArea = request.MinArea,
            MaxArea = request.MaxArea,
            RoomCount = request.RoomCount,
            Properties = properties
        };

        return View(model);
    }

    // Добавление объекта в подбор
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int requestId, int propertyId)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var realtor = await _dbContext.RealtorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        var request = await _dbContext.Requests
            .FirstOrDefaultAsync(item =>
                item.Id == requestId &&
                item.RealtorProfileId == realtor.Id);

        if (request == null)
        {
            return NotFound();
        }

        var property = await _dbContext.Properties
            .FirstOrDefaultAsync(item =>
                item.Id == propertyId &&
                item.Status == PropertyStatus.Available);

        if (property == null)
        {
            TempData["ErrorMessage"] =
                "Объект не найден или больше недоступен.";

            return RedirectToAction(
                nameof(ForRequest),
                new { requestId });
        }

        bool alreadySelected = await _dbContext.Selections
            .AnyAsync(selection =>
                selection.RequestId == requestId &&
                selection.PropertyId == propertyId);

        if (alreadySelected)
        {
            TempData["ErrorMessage"] =
                "Этот объект уже добавлен в подбор.";

            return RedirectToAction(
                nameof(ForRequest),
                new { requestId });
        }

        var selection = new SelectionEntity
        {
            RequestId = requestId,
            PropertyId = propertyId,
            CreatedAt = DateTime.UtcNow,
            IsViewed = false
        };

        _dbContext.Selections.Add(selection);

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Объект успешно добавлен в подбор для клиента.";

        return RedirectToAction(
            nameof(ForRequest),
            new { requestId });
    }

    // Просмотр подборок клиентом
    [Authorize(Roles = UserRoles.Client)]
    [HttpGet]
    public async Task<IActionResult> MySelections()
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
            .Include(request => request.Selections)
                .ThenInclude(selection => selection.Property)
                    .ThenInclude(property => property.Photos)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();

        var model = new MySelectionsViewModel
        {
            Requests = requests.Select(request =>
                new MySelectionRequestViewModel
                {
                    RequestId = request.Id,
                    DealType = request.DealType,
                    RequestStatus = request.Status,
                    CreatedAt = request.CreatedAt,
                    Properties = request.Selections
                        .OrderByDescending(selection => selection.CreatedAt)
                        .Select(selection => new MySelectionPropertyViewModel
                        {
                            SelectionId = selection.Id,
                            PropertyId = selection.Property.Id,
                            Address = selection.Property.Address,
                            District = selection.Property.District,
                            Type = selection.Property.Type,
                            Area = selection.Property.Area,
                            RoomCount = selection.Property.RoomCount,
                            Price = selection.Property.Price,
                            Status = selection.Property.Status,
                            MainPhotoPath = selection.Property.Photos
                                .Where(photo => photo.IsMain)
                                .Select(photo => photo.FilePath)
                                .FirstOrDefault(),
                            AddedAt = selection.CreatedAt,
                            IsViewed = selection.IsViewed,
                            ClientComment = selection.ClientComment
                        })
                        .ToList()
                })
                .ToList()
        };

        return View(model);
    }

    // Клиент отмечает объект просмотренным и оставляет комментарий
    [Authorize(Roles = UserRoles.Client)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateFeedback(
        int selectionId,
        string? comment)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var client = await _dbContext.ClientProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (client == null)
        {
            return Forbid();
        }

        var selection = await _dbContext.Selections
            .Include(item => item.Request)
            .FirstOrDefaultAsync(item =>
                item.Id == selectionId &&
                item.Request.ClientProfileId == client.Id);

        if (selection == null)
        {
            return NotFound();
        }

        if (comment?.Length > 1000)
        {
            TempData["ErrorMessage"] =
                "Комментарий не должен превышать 1000 символов.";

            return RedirectToAction(nameof(MySelections));
        }

        selection.IsViewed = true;
        selection.ClientComment = comment?.Trim();

        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Ваш отзыв успешно сохранён.";

        return RedirectToAction(nameof(MySelections));
    }
}
