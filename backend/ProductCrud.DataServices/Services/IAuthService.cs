using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Models.Auth;

namespace ProductCrud.DataServices.Services;

public interface IAuthService
{
    Task<ResultModel<LoginResponseDTO>> LoginAsync(LoginModel model);
}
