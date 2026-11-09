public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<int> CreateRoleAsync(CreateRoleDto model);
}