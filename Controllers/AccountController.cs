
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Data;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Profiles;
using RealEstateAgency.ViewModels;

namespace RealEstateAgency.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Role != UserRoles.Client &&
            model.Role != UserRoles.Realtor)
        {
            ModelState.AddModelError(
                nameof(model.Role),
                "Выбран недопустимый тип аккаунта.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var createResult = await _userManager.CreateAsync(
            user,
            model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            model.Role);

        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            return View(model);
        }

        if (model.Role == UserRoles.Client)
        {
            _dbContext.ClientProfiles.Add(new ClientProfile
            {
                UserId = user.Id
            });
        }
        else if (model.Role == UserRoles.Realtor)
        {
            _dbContext.RealtorProfiles.Add(new RealtorProfile
            {
                UserId = user.Id
            });
        }

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        await _signInManager.SignInAsync(user, isPersistent: false);

        return await RedirectToDashboardAsync(user);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(
            model.Email.Trim());

        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                "Неверная электронная почта или пароль.");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) &&
                Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return await RedirectToDashboardAsync(user);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Аккаунт временно заблокирован из-за большого количества неудачных попыток входа.");
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Неверная электронная почта или пароль.");
        }

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<IActionResult> RedirectToDashboardAsync(
        ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(user, UserRoles.Admin))
        {
            return RedirectToAction("Admin", "Dashboard");
        }

        if (await _userManager.IsInRoleAsync(user, UserRoles.Realtor))
        {
            return RedirectToAction("Realtor", "Dashboard");
        }

        if (await _userManager.IsInRoleAsync(user, UserRoles.Client))
        {
            return RedirectToAction("Client", "Dashboard");
        }

        return RedirectToAction("Index", "Home");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }
}
