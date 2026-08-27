namespace ProductCrud.DataServices.Infrastructure;

public interface ICurrentUserService
{
    int? UserId { get; }
    string Username { get; }
    string Role { get; }
}
