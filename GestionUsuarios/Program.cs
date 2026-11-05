using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Base de Datos SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero // Evita desfases de tiempo en la expiración del token
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Evita la respuesta por defecto de ASP.NET Core
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            
            var result = JsonSerializer.Serialize(new { message = "No estás autenticado. Por favor, inicia sesión para obtener un token." });
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            
            var result = JsonSerializer.Serialize(new { message = "Acceso denegado. No tienes los permisos o roles necesarios para realizar esta acción." });
            return context.Response.WriteAsync(result);
        }
    };

});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. Configurar Swagger con el Botón "Authorize" para enviar el Token Bearer   
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese el token JWT en el formato: Bearer {su_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { 
            error = "Error interno del servidor.", details = ex.Message
        });
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any(u => u.Username == "superadmin"))
    {
        var superAdminUser = new User
        {
            Username = "superadmin",
            Email = "admin@sistema.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*")
        };
        db.Users.Add(superAdminUser);
        db.SaveChanges();


        // Asociar rol SuperAdmin (Id = 1)
        db.UserRoles.Add(new UserRole
        {
            UserId = superAdminUser.Id,
            RoleId = 1
        });
        db.SaveChanges();
    }
}


app.Run();  