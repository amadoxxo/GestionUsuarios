using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                CreatedAt = u.CreateAt,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        return await _context.Users
                .Where(u => u.Id == id)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    CreatedAt = u.CreateAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();
    }

    public async Task<int> CreateUserAsync(CreateUserDto model)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = hashedPassword,
            CreateAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var defaultRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");

        if (defaultRole != null)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            });

            await _context.SaveChangesAsync();
        }

        return user.Id;
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto model)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.Username = model.Username;
        user.Email = model.Email;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        // Validar que existan tanto el usuario como el rol
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);

        if (!userExists || !roleExists) return false;

        // Verificar si ya tiene el rol asignado para evitar duplicados
        var alreadyHasRole = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (alreadyHasRole) return true;

        _context.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = roleId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();

        return true;
    }

}