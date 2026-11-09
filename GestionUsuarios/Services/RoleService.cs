using Microsoft.EntityFrameworkCore;

public class RoleService: IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        return await _context.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();
    }

    public async Task<int> CreateRoleAsync(CreateRoleDto model)
    {
        // Validar si el rol ya existe para evitar duplicados
        var roleExists = await _context.Roles
            .AnyAsync(r => r.Name.ToLower() == model.Name.ToLower());

        if (roleExists)
        {
            throw new InvalidOperationException($"El rol '{model.Name}' ya existe en el sistema.");
        }

        var role = new Role
        {
            Name = model.Name
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return role.Id;
    }
}