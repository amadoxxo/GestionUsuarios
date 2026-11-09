using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetUsers()
    {   
        var user = await _userService.GetAllUsersAsync();

        return Ok(user);
    } 

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null) return NotFound(new { message = "Usuario no encontrado." });
        
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
    {
        var userId = await _userService.CreateUserAsync(model);
        return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { message = "Usuario creado exitosamente.", userId });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto model)
    {
        var updated = await _userService.UpdateUserAsync(id, model);
        
        if (!updated) return NotFound(new { message = "Usuario no encontrado." });
        
        return Ok(new { 
            message = "Usuario actualizado correctamente."
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _userService.DeleteUserAsync(id);
        
        if (!deleted) return NotFound(new { message = "Usuario no encontrado." });
        
        return Ok(new {
            message = "Usuario eliminado correctamente."
        });
    }

    [HttpPost("{id}/roles/{roleId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AssignRole(int id, int roleId)
    {
        var success = await _userService.AssignRoleToUserAsync(id, roleId);
        
        if (!success) return BadRequest(new { message = "No se pudo asignar el rol. Verifique los IDs." });

        return Ok(new {
            message = "Rol asignado exitosamente al usuario."
        });
    }

    [HttpDelete("{id}/roles/{roleId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RemoveRole(int id, int roleId)
    {
        var success = await _userService.RemoveRoleFromUserAsync(id, roleId);

        if (!success) return NotFound(new { message = "La relación usuario-rol no existe." });
        
        return Ok(new {
            message = "Rol removido exitosamente del usuario."
        });
    }
}