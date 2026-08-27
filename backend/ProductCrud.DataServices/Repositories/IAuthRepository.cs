using ProductCrud.DataServices.Entities;

namespace ProductCrud.DataServices.Repositories;

public interface IAuthRepository
{
    Task<AppUserEntity?> GetByUsernameAsync(string username);
}
