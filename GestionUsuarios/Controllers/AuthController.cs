using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var token = await _authService.LoginAsync(model);
        if (token == null) return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        return Ok(new { token });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUserInfo()
    {
        // Extraer los claims del usuario autenticado actual desde el Token JWT
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToList();

        return Ok(new { 
            userId,
            username,
            roles
        });
    }
}