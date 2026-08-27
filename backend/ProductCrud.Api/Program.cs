using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using ProductCrud.Api.Infrastructure;
using ProductCrud.Api.Middleware;
using ProductCrud.Api.Models;
using ProductCrud.Api.Services;

using ProductCrud.DataServices.Data;
using ProductCrud.DataServices.Entities;
using ProductCrud.DataServices.Infrastructure;
using ProductCrud.DataServices.Repositories;
using ProductCrud.DataServices.Services;

using ProductCrud.Api.Infrastructure.Caching;
using ProductCrud.DataServices.Infrastructure.Caching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService,MemoryCacheService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditLogInterceptor>();

builder.Services.AddDbContext<ProductCrudDbContext>((serviceProvider, options) =>
{
    var auditLogInterceptor = serviceProvider.GetRequiredService<AuditLogInterceptor>();

    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));

    options.AddInterceptors(auditLogInterceptor);
});

builder.Services.AddScoped<IProductManagementRepository, ProductManagementRepository>();
builder.Services.AddScoped<IProductManagementService, ProductManagementService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IPasswordHasher<AppUserEntity>, PasswordHasher<AppUserEntity>>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var message = actionContext.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault();

            var traceId = actionContext.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(new ApiErrorResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(message)
                    ? "Dữ liệu gửi lên không hợp lệ."
                    : message,
                StatusCode = StatusCodes.Status400BadRequest,
                TraceId = traceId
            });
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token để gọi API cần đăng nhập."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;

    if (response.ContentLength.HasValue || !string.IsNullOrEmpty(response.ContentType))
    {
        return;
    }

    response.ContentType = "application/json";

    var message = response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => "Bạn chưa đăng nhập hoặc token đã hết hạn.",
        StatusCodes.Status403Forbidden => "Bạn không có quyền thực hiện chức năng này.",
        StatusCodes.Status404NotFound => "Không tìm thấy tài nguyên yêu cầu.",
        StatusCodes.Status413PayloadTooLarge => "File vượt quá kích thước cho phép.",
        _ => "Yêu cầu không thể xử lý."
    };

    var errorResponse = new ApiErrorResponse
    {
        Success = false,
        Message = message,
        StatusCode = response.StatusCode,
        TraceId = statusCodeContext.HttpContext.TraceIdentifier
    };

    await response.WriteAsync(
        JsonSerializer.Serialize(
            errorResponse,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseCors("Angular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DbInitializer.InitializeAsync(app.Services);

app.Run();
