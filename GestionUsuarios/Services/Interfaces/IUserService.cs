public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<int> CreateUserAsync(CreateUserDto model);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto model);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> AssignRoleToUserAsync(int userId, int roleId);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
}