namespace ProductCrud.Api.Infrastructure;

public interface ICurrentUserService
{
    int? UserId { get; }
    string Username { get; }
    string Role { get; }
}
