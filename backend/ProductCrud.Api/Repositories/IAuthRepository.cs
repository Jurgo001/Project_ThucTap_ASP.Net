using ProductCrud.Api.Models.Entities;

namespace ProductCrud.Api.Repositories;

public interface IAuthRepository
{
    Task<AppUserEntity?> GetByUsernameAsync(string username);
}
