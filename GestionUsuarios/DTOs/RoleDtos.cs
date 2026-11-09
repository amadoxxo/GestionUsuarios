public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
}