using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterDto model)
    {
        // 1. Validar modelo explícitamente (opcional si usas [ApiController], pero refuerza)
        if (model == null)
        {
            return BadRequest(new { message = "Los datos del usuario son requeridos." });
        }

        // 2. Verificar si el Username o el Email ya existen
        if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
        {
            return BadRequest(new { message = "El nombre de usuario o el correo electrónico ya están registrados." });
        }

        // 3. Buscar el rol por defecto antes de iniciar cambios
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole == null)
        {
            return StatusCode(500, new { message = "Error de configuración: El rol por defecto 'User' no existe en el sistema." });
        }

        // 4. Usar una Transacción de EF Core para asegurar atomicidad (Usuario + Rol)
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = defaultRole.Id
            });
            await _context.SaveChangesAsync();

            // Si todo sale bien, confirmamos la transacción
            await transaction.CommitAsync();

            return Ok(new 
            { 
                message = "Usuario registrado exitosamente por el SuperAdministrador.",
                userId = user.Id
            });
        }
        catch (DbUpdateException dbEx)
        {
            // Revertir cambios en la BD si algo falla
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error en la base de datos al guardar el usuario.", details = dbEx.Message });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error interno del servidor.", details = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.CreateAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener la lista de usuarios.", details = ex.Message });
        }
    }
}