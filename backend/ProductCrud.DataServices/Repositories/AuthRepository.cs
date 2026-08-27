using Microsoft.EntityFrameworkCore;
using ProductCrud.DataServices.Data;
using ProductCrud.DataServices.Entities;

namespace ProductCrud.DataServices.Repositories;

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
