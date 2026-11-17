using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Enums;
using RealEstateAgency.Models.Profiles;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

[Authorize]
public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ContractsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Создание договора на основании подбора.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public async Task<IActionResult> Create(int selectionId)
    {
        var realtor = await GetCurrentRealtorAsync();

        if (realtor == null)
        {
            return Forbid();
        }

        var selection = await GetSelectionAsync(selectionId, realtor.Id);

        if (selection == null)
        {
            return NotFound();
        }

        if (selection.Property.Status != PropertyStatus.Available)
        {
            TempData["Error"] =
                "Этот объект уже недоступен для заключения договора.";

            return RedirectToAction(
                "RealtorSelections",
                "Selections");
        }

        var contractExists = await _context.Contracts
            .AnyAsync(c => c.PropertyId == selection.PropertyId);

        if (contractExists)
        {
            TempData["Error"] =
                "Для этого объекта уже создан договор.";

            return RedirectToAction(
                "RealtorSelections",
                "Selections");
        }

        return View(BuildCreateViewModel(selection, realtor));
    }

    // Сохранение нового договора.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContractCreateViewModel model)
    {
        var realtor = await GetCurrentRealtorAsync();

        if (realtor == null)
        {
            return Forbid();
        }

        var selection = await GetSelectionAsync(
            model.SelectionId,
            realtor.Id);

        if (selection == null)
        {
            return NotFound();
        }

        if (selection.Property.Status != PropertyStatus.Available)
        {
            TempData["Error"] =
                "Этот объект уже недоступен для заключения договора.";

            return RedirectToAction(
                "RealtorSelections",
                "Selections");
        }

        var contractExists = await _context.Contracts
            .AnyAsync(c => c.PropertyId == selection.PropertyId);

        if (contractExists)
        {
            TempData["Error"] =
                "Для этого объекта уже создан договор.";

            return RedirectToAction(
                "RealtorSelections",
                "Selections");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildCreateViewModel(
                selection,
                realtor,
                model.Notes));
        }

        var commissionAmount = decimal.Round(
            selection.Property.Price * realtor.CommissionPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);

        var contract = new Contract
        {
            ContractNumber =
                $"ДП-{DateTime.UtcNow:yyyyMMdd}-{selection.Id:D6}",

            ClientProfileId = selection.Request.ClientProfileId,
            RealtorProfileId = realtor.Id,
            PropertyId = selection.PropertyId,

            DealType = selection.Request.DealType,
            Status = ContractStatus.Draft,

            PropertyPrice = selection.Property.Price,
            CommissionPercent = realtor.CommissionPercent,
            CommissionAmount = commissionAmount,

            CreatedAt = DateTime.UtcNow,
            Notes = model.Notes?.Trim()
        };

        _context.Contracts.Add(contract);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Договор {contract.ContractNumber} успешно создан.";

        return RedirectToAction(nameof(RealtorIndex));
    }

    // Все договоры текущего риелтора.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpGet]
    public async Task<IActionResult> RealtorIndex()
    {
        var realtor = await GetCurrentRealtorAsync();

        if (realtor == null)
        {
            return Forbid();
        }

        var contracts = await _context.Contracts
            .AsNoTracking()
            .Where(c => c.RealtorProfileId == realtor.Id)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContractListItemViewModel
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,

                ClientName =
                    c.Client.User.FirstName + " " +
                    c.Client.User.LastName,

                RealtorName =
                    c.Realtor.User.FirstName + " " +
                    c.Realtor.User.LastName,

                PropertyAddress = c.Property.Address,
                PropertyPrice = c.PropertyPrice,
                CommissionPercent = c.CommissionPercent,
                CommissionAmount = c.CommissionAmount,

                DealType = c.DealType,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return View(new ContractListViewModel
        {
            Contracts = contracts
        });
    }

    // Договоры текущего клиента.
    [Authorize(Roles = UserRoles.Client)]
    [HttpGet]
    public async Task<IActionResult> MyContracts()
    {
        var userId = _userManager.GetUserId(User);

        var client = await _context.ClientProfiles
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (client == null)
        {
            return Forbid();
        }

        var contracts = await _context.Contracts
            .AsNoTracking()
            .Where(c => c.ClientProfileId == client.Id)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContractListItemViewModel
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,

                ClientName =
                    c.Client.User.FirstName + " " +
                    c.Client.User.LastName,

                RealtorName =
                    c.Realtor.User.FirstName + " " +
                    c.Realtor.User.LastName,

                PropertyAddress = c.Property.Address,
                PropertyPrice = c.PropertyPrice,
                CommissionPercent = c.CommissionPercent,
                CommissionAmount = c.CommissionAmount,

                DealType = c.DealType,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return View(new ContractListViewModel
        {
            Contracts = contracts
        });
    }

    // Подробная информация о договоре.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Client)
                .ThenInclude(c => c.User)
            .Include(c => c.Realtor)
                .ThenInclude(r => r.User)
            .Include(c => c.Property)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            return NotFound();
        }

        if (User.IsInRole(UserRoles.Realtor))
        {
            var realtor = await GetCurrentRealtorAsync();

            if (realtor == null ||
                contract.RealtorProfileId != realtor.Id)
            {
                return Forbid();
            }
        }
        else if (User.IsInRole(UserRoles.Client))
        {
            var userId = _userManager.GetUserId(User);

            var client = await _context.ClientProfiles
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (client == null ||
                contract.ClientProfileId != client.Id)
            {
                return Forbid();
            }
        }
        else if (!User.IsInRole(UserRoles.Admin))
        {
            return Forbid();
        }

        return View(contract);
    }

    // Изменение статуса договора риелтором.
    [Authorize(Roles = UserRoles.Realtor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        ContractStatus status)
    {
        if (!Enum.IsDefined(typeof(ContractStatus), status))
        {
            return BadRequest();
        }

        var realtor = await GetCurrentRealtorAsync();

        if (realtor == null)
        {
            return Forbid();
        }

        var contract = await _context.Contracts
            .Include(c => c.Property)
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.RealtorProfileId == realtor.Id);

        if (contract == null)
        {
            return NotFound();
        }

        if (contract.Status == status)
        {
            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        if (!IsAllowedStatusTransition(contract.Status, status))
        {
            TempData["Error"] =
                "Выбран недопустимый переход статуса договора.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        contract.Status = status;

        if (status == ContractStatus.Signed)
        {
            contract.SignedAt = DateTime.UtcNow;
            contract.Property.Status = PropertyStatus.Reserved;
        }

        if (status == ContractStatus.InProgress)
        {
            contract.Property.Status = PropertyStatus.Reserved;
        }

        if (status == ContractStatus.Completed)
        {
            contract.CompletedAt = DateTime.UtcNow;

            contract.Property.Status =
                contract.DealType == DealType.PurchaseSale
                    ? PropertyStatus.Sold
                    : PropertyStatus.Rented;

            realtor.CompletedDealsCount++;
        }

        if (status == ContractStatus.Terminated)
        {
            contract.TerminatedAt = DateTime.UtcNow;
            contract.Property.Status = PropertyStatus.Available;

            if (contract.CompletedAt.HasValue)
            {
                realtor.CompletedDealsCount =
                    Math.Max(0, realtor.CompletedDealsCount - 1);
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Статус договора успешно изменён.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // Получение профиля текущего риелтора.
    private async Task<RealtorProfile?> GetCurrentRealtorAsync()
    {
        var userId = _userManager.GetUserId(User);

        return await _context.RealtorProfiles
            .FirstOrDefaultAsync(r => r.UserId == userId);
    }

    // Получение подбора с проверкой принадлежности риелтору.
    private async Task<Selection?> GetSelectionAsync(
        int selectionId,
        int realtorProfileId)
    {
        return await _context.Selections
            .Include(s => s.Request)
                .ThenInclude(r => r.Client)
                    .ThenInclude(c => c.User)
            .Include(s => s.Property)
            .FirstOrDefaultAsync(s =>
                s.Id == selectionId &&
                s.Request.RealtorProfileId == realtorProfileId);
    }

    // Подготовка данных формы создания договора.
    private static ContractCreateViewModel BuildCreateViewModel(
        Selection selection,
        RealtorProfile realtor,
        string? notes = null)
    {
        var commissionAmount = decimal.Round(
            selection.Property.Price * realtor.CommissionPercent / 100m,
            2,
            MidpointRounding.AwayFromZero);

        return new ContractCreateViewModel
        {
            SelectionId = selection.Id,

            ContractNumber =
                $"ДП-{DateTime.UtcNow:yyyyMMdd}-{selection.Id:D6}",

            ClientName =
                $"{selection.Request.Client.User.FirstName} " +
                $"{selection.Request.Client.User.LastName}",

            PropertyAddress = selection.Property.Address,
            DealType = selection.Request.DealType,

            PropertyPrice = selection.Property.Price,
            CommissionPercent = realtor.CommissionPercent,
            CommissionAmount = commissionAmount,

            Notes = notes
        };
    }

    // Разрешённые переходы между статусами договора.
    private static bool IsAllowedStatusTransition(
        ContractStatus current,
        ContractStatus next)
    {
        return current switch
        {
            ContractStatus.Draft =>
                next == ContractStatus.UnderReview ||
                next == ContractStatus.Terminated,

            ContractStatus.UnderReview =>
                next == ContractStatus.Signed ||
                next == ContractStatus.Terminated,

            ContractStatus.Signed =>
                next == ContractStatus.InProgress ||
                next == ContractStatus.Terminated,

            ContractStatus.InProgress =>
                next == ContractStatus.Completed ||
                next == ContractStatus.Terminated,

            ContractStatus.Completed =>
                next == ContractStatus.Terminated,

            _ => false
        };
    }
}