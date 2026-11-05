using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous] // Asegura que el login sea público
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        try
        {
            // 1. Validación básica del modelo recibido
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { message = "El usuario y la contraseña son obligatorios." });
            }

            // 2. Buscar al usuario incluyendo sus roles de forma eficiente
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            // 3. Validar existencia del usuario y verificación segura de la contraseña con BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Usuario o contraseña incorrectos." });
            }

            // 4. Leer y validar la configuración de JWT de forma segura
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var durationMinutes = jwtSettings["DurationInMinutes"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(durationMinutes))
            {
                return StatusCode(500, new { message = "Error de configuración del servidor: Faltan parámetros JWT." });
            }

            var key = Encoding.ASCII.GetBytes(secretKey);

            // 5. Construir los Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null && !string.IsNullOrEmpty(userRole.Role.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                }
            }

            // 6. Generar el descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(durationMinutes)),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // 7. Respuesta exitosa con el token
            return Ok(new
            {
                message = "Inicio de sesión exitoso.",
                token = tokenString
            });
        }
        catch (Exception ex)
        {
            // Captura cualquier error imprevisto (ej. fallo de base de datos) devolviendo un 500 limpio
            return StatusCode(500, new { message = "Error interno del servidor al procesar el inicio de sesión.", details = ex.Message });
        }
    }
}