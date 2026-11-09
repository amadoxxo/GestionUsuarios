public interface IAuthService
{
    Task<string?> LoginAsync(LoginDto model);
}