using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public HomeController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Главная страница с реальными объектами из базы данных.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var properties = await _dbContext.Properties
            .AsNoTracking()
            .Where(property =>
                property.Status != PropertyStatus.PendingModeration &&
                property.Status != PropertyStatus.Rejected)
            .OrderByDescending(property => property.CreatedAt)
            .Take(6)
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

        var model = new HomeViewModel
        {
            Properties = properties
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id
                ?? HttpContext.TraceIdentifier
        });
    }
}