using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCrud.Api.Models.Auth;
using ProductCrud.Api.Services;

namespace ProductCrud.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthorizationController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        return Ok(await _authService.LoginAsync(model));
    }
}
