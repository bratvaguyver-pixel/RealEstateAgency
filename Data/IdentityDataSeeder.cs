using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Profiles;

namespace RealEstateAgency.Data;

public static class IdentityDataSeeder
{
    public static async Task SeedRolesAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var services = scope.ServiceProvider;

        var roleManager = services
            .GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        {
            UserRoles.Client,
            UserRoles.Realtor,
            UserRoles.Admin
        };

        // Создаём необходимые роли.
        foreach (var role in roles)
        {
            bool roleExists = await roleManager.RoleExistsAsync(role);

            if (!roleExists)
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    string errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Не удалось создать роль {role}: {errors}");
                }
            }
        }

        // При наличии настроек создаём первого администратора.
        await SeedAdminAsync(services);
    }

    private static async Task SeedAdminAsync(
        IServiceProvider services)
    {
        var configuration = services
            .GetRequiredService<IConfiguration>();

        string? email =
            configuration["AdminBootstrap:Email"];

        string? password =
            configuration["AdminBootstrap:Password"];

        // Если настройки не заданы, создание администратора пропускается.
        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        // Если заполнена только часть настроек, сообщаем об ошибке.
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Для создания администратора необходимо указать " +
                "AdminBootstrap:Email и AdminBootstrap:Password.");
        }

        email = email.Trim();

        var userManager = services
            .GetRequiredService<UserManager<ApplicationUser>>();

        var dbContext = services
            .GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName =
                    configuration["AdminBootstrap:FirstName"]
                    ?? "Администратор",
                LastName =
                    configuration["AdminBootstrap:LastName"]
                    ?? "ДомПрофи",
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(
                user,
                password);

            if (!createResult.Succeeded)
            {
                string errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Не удалось создать администратора: {errors}");
            }
        }
        else
        {
            // Не назначаем административную роль существующему
            // обычному пользователю автоматически.
            bool isAdmin = await userManager.IsInRoleAsync(
                user,
                UserRoles.Admin);

            if (!isAdmin)
            {
                throw new InvalidOperationException(
                    "Указанная электронная почта уже занята обычным " +
                    "пользователем. Для первого администратора " +
                    "используйте отдельную электронную почту.");
            }
        }

        // Назначаем административную роль.
        if (!await userManager.IsInRoleAsync(user, UserRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user,
                UserRoles.Admin);

            if (!roleResult.Succeeded)
            {
                string errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Не удалось назначить роль администратора: {errors}");
            }
        }

        // Создаём профиль администратора, если его ещё нет.
        bool profileExists = await dbContext.AdminProfiles
            .AnyAsync(profile => profile.UserId == user.Id);

        if (!profileExists)
        {
            dbContext.AdminProfiles.Add(new AdminProfile
            {
                UserId = user.Id,
                HasFullAccess = true
            });

            await dbContext.SaveChangesAsync();
        }
    }
}