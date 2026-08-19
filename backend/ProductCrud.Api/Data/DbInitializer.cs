using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductCrud.Api.Models.Entities;

namespace ProductCrud.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ProductCrudDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUserEntity>>();

        if (!await dbContext.AppUsers.AnyAsync())
        {
            var users = new[]
            {
                CreateUser("admin", "Admin@123", "Admin", passwordHasher),
                CreateUser("editor", "Editor@123", "Editor", passwordHasher),
                CreateUser("viewer", "Viewer@123", "Viewer", passwordHasher)
            };

            dbContext.AppUsers.AddRange(users);
            await dbContext.SaveChangesAsync();
        }
    }

    private static AppUserEntity CreateUser(
        string username,
        string password,
        string role,
        IPasswordHasher<AppUserEntity> passwordHasher)
    {
        var user = new AppUserEntity
        {
            Username = username,
            Role = role,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        return user;
    }
}
