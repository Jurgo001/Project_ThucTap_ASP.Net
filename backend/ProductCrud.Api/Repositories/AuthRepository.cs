using Microsoft.EntityFrameworkCore;
using ProductCrud.Api.Data;
using ProductCrud.Api.Models.Entities;

namespace ProductCrud.Api.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ProductCrudDbContext _dbContext;

    public AuthRepository(ProductCrudDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppUserEntity?> GetByUsernameAsync(string username)
    {
        var normalizedUsername = username.Trim();

        return await _dbContext.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user =>
                user.Username == normalizedUsername &&
                user.IsActive);
    }
}
