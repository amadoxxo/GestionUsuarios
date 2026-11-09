using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRoles() => Ok(await _roleService.GetAllRolesAsync());

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto model)
    {
        var roleId = await _roleService.CreateRoleAsync(model);
        return Ok(new { message = "Rol creado exitosamente.", roleId });
    }
}