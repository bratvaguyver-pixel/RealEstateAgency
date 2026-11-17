
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    private const long MaxPhotoSize = 5 * 1024 * 1024;
    private const int MaxPhotoCount = 10;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public PropertiesController(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    // Каталог недвижимости доступен всем посетителям.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(PropertyFilterViewModel filter)
    {
        var query = _dbContext.Properties
            .AsNoTracking()
            .Include(property => property.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.District))
        {
            string district = filter.District.Trim().ToLower();

            query = query.Where(property =>
                property.District.ToLower().Contains(district));
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(property =>
                property.Type == filter.Type.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(property =>
                property.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(property =>
                property.Price <= filter.MaxPrice.Value);
        }

        if (filter.MinArea.HasValue)
        {
            query = query.Where(property =>
                property.Area >= filter.MinArea.Value);
        }

        if (filter.MaxArea.HasValue)
        {
            query = query.Where(property =>
                property.Area <= filter.MaxArea.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(property =>
                property.Status == filter.Status.Value);
        }

        var properties = await query
            .OrderByDescending(property => property.CreatedAt)
            .Select(property => new PropertyListItemViewModel
            {
                Id = property.Id,
                Address = property.Address,
                District = property.District,
                Type = property.Type,
                Area = property.Area,
                RoomCount = property.RoomCount,
                Price = property.Price,
                Status = property.Status,
                MainPhotoPath = property.Photos
                    .Where(photo => photo.IsMain)
                    .Select(photo => photo.FilePath)
                    .FirstOrDefault()
            })
            .ToListAsync();

        filter.Properties = properties;

        return View(filter);
    }

    // Подробная информация об объекте.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var property = await _dbContext.Properties
            .AsNoTracking()
            .Include(item => item.Photos)
            .Include(item => item.Realtor)
                .ThenInclude(realtor => realtor.User)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (property == null)
        {
            return NotFound();
        }

        var model = new PropertyDetailsViewModel
        {
            Id = property.Id,
            Address = property.Address,
            District = property.District,
            Type = property.Type,
            Area = property.Area,
            RoomCount = property.RoomCount,
            Floor = property.Floor,
            Price = property.Price,
            Status = property.Status,
            Description = property.Description,
            CreatedAt = property.CreatedAt,
            RealtorName =
                $"{property.Realtor.User.FirstName} {property.Realtor.User.LastName}",
            PhotoPaths = property.Photos
                .OrderByDescending(photo => photo.IsMain)
                .ThenBy(photo => photo.UploadedAt)
                .Select(photo => photo.FilePath)
                .ToList()
        };

        return View(model);
    }

    // Форма добавления объекта доступна только риелторам.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new PropertyCreateViewModel());
    }

    // Сохранение нового объекта.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyCreateViewModel model)
    {
        ValidatePhotos(model.Photos);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var realtor = await _dbContext.RealtorProfiles
            .FirstOrDefaultAsync(profile => profile.UserId == userId);

        if (realtor == null)
        {
            return Forbid();
        }

        var property = new Property
        {
            Address = model.Address.Trim(),
            District = model.District.Trim(),
            Type = model.Type,
            Area = model.Area,
            RoomCount = model.RoomCount,
            Floor = model.Floor,
            Price = model.Price,
            Description = model.Description?.Trim(),
            Status = PropertyStatus.Available,
            CreatedAt = DateTime.UtcNow,
            RealtorProfileId = realtor.Id
        };

        _dbContext.Properties.Add(property);

        await _dbContext.SaveChangesAsync();

        if (model.Photos.Count > 0)
        {
            var uploadDirectory = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "properties");

            Directory.CreateDirectory(uploadDirectory);

            for (int i = 0; i < model.Photos.Count; i++)
            {
                var photo = model.Photos[i];

                string extension = Path.GetExtension(photo.FileName)
                    .ToLowerInvariant();

                string fileName = $"{Guid.NewGuid()}{extension}";

                string filePath = Path.Combine(uploadDirectory, fileName);

                await using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                _dbContext.PropertyPhotos.Add(new PropertyPhoto
                {
                    PropertyId = property.Id,
                    FilePath = $"/uploads/properties/{fileName}",
                    IsMain = i == 0,
                    UploadedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Объект недвижимости успешно добавлен.";

        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    private void ValidatePhotos(List<IFormFile> photos)
    {
        if (photos.Count > MaxPhotoCount)
        {
            ModelState.AddModelError(
                nameof(PropertyCreateViewModel.Photos),
                $"Можно загрузить не более {MaxPhotoCount} фотографий.");

            return;
        }

        foreach (var photo in photos)
        {
            if (photo.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(PropertyCreateViewModel.Photos),
                    "Один из выбранных файлов пуст.");

                continue;
            }

            if (photo.Length > MaxPhotoSize)
            {
                ModelState.AddModelError(
                    nameof(PropertyCreateViewModel.Photos),
                    "Размер одной фотографии не должен превышать 5 МБ.");

                continue;
            }

            string extension = Path.GetExtension(photo.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(PropertyCreateViewModel.Photos),
                    "Допускаются только изображения JPG, JPEG, PNG и WEBP.");
            }
        }
    }
}
