using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProductCrud.DataServices.Models;
using ProductCrud.DataServices.Models.Auth;
using ProductCrud.DataServices.Entities;
using ProductCrud.DataServices.Repositories;
using Microsoft.Extensions.Configuration;

namespace ProductCrud.DataServices.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher<AppUserEntity> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IAuthRepository authRepository,
        IPasswordHasher<AppUserEntity> passwordHasher,
        IConfiguration configuration)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<ResultModel<LoginResponseDTO>> LoginAsync(LoginModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.Password))
        {
            throw new ArgumentException("Vui lòng nhập tên đăng nhập và mật khẩu.");
        }

        var user = await _authRepository.GetByUsernameAsync(model.Username);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            model.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        var expiresMinutes = _configuration.GetValue<int?>("Jwt:ExpiresMinutes") ?? 120;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var response = new LoginResponseDTO
        {
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            Token = CreateToken(user, expiresAtUtc),
            ExpiresAtUtc = expiresAtUtc
        };

        return ResultModel<LoginResponseDTO>.Ok(response, "Đăng nhập thành công.");
    }

    private string CreateToken(AppUserEntity user, DateTime expiresAtUtc)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
