using ProductCrud.Api.Models;
using ProductCrud.Api.Models.Auth;

namespace ProductCrud.Api.Services;

public interface IAuthService
{
    Task<ResultModel<LoginResponseDTO>> LoginAsync(LoginModel model);
}
