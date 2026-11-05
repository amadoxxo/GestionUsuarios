# 🛡️ User Management API (.NET 10 & SQL Server)

API RESTful desarrollada en **ASP.NET Core (.NET 10)** para la gestión segura de usuarios, control de roles (RBAC) y autenticación basada en JSON Web Tokens (JWT). Utiliza Entity Framework Core como ORM y Docker para aislar la persistencia de la base de datos en SQL Server.

---

## 🚀 Características Principales

* **Autenticación JWT:** Emisión de tokens seguros con validación de expiración, *Issuer* y *Audience*.
* **Control de Accesos Basado en Roles (RBAC):** Endpoints protegidos para los roles `SuperAdmin` y `Admin`.
* **Seguridad de Contraseñas:** Hash y verificación segura mediante **BCrypt**.
* **Base de datos en Docker:** SQL Server 2022 corriendo de forma aislada mediante Docker Compose.
* **Transacciones Atómicas:** Operaciones críticas (como registro de usuarios con asignación de roles) envueltas en transacciones de EF Core para garantizar la integridad de los datos.
* **Documentación Interactiva:** Integración con Swagger UI.

---

## 🛠️ Tecnologías Utilizadas

* **Framework:** .NET 10 (ASP.NET Core Web API)
* **ORM:** Entity Framework Core
* **Base de Datos:** SQL Server 2022 (vía Docker)
* **Seguridad:** `Microsoft.AspNetCore.Authentication.JwtBearer` & `BCrypt.Net-Next`

---

## 📋 Requisitos Previos

Asegúrate de tener instalado en tu equipo:
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker Desktop](https://www.docker.com/) (para levantar SQL Server)

---

## ⚙️ Configuración y Puesta en Marcha

### 1. Clonar el repositorio y configurar la base de datos
Levanta el contenedor de SQL Server utilizando Docker Compose en la raíz del proyecto:

```bash
docker compose up -d
```

### 2. Configurar `appsettings.json`
Asegúrate de que tu archivo `appsettings.json` tenga la cadena de conexión apuntando a Docker y los parámetros de JWT configurados (la clave secreta debe tener al menos 32 caracteres):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=UserManagementDb;User Id=sa;Password=PasswordSuperSeguro123*;Trusted_Connection=False;Encrypt=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "EstaEsUnaClaveSecretaSuperSeguraDeMasDe32CaracteresParaEvitarErroresJWT123!",
    "Issuer": "UserManagementApi",
    "Audience": "UserManagementClient",
    "DurationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Ejecutar la aplicación
Ejecuta el proyecto con el comando:

```bash
dotnet run
```

La API estará corriendo en `http://localhost:5000` (o el puerto asignado por tu entorno) y podrás acceder a la documentación de Swagger en `http://localhost:5000/swagger`.

---

## 📊 Modelo Entidad-Relación (ER)

El sistema gestiona usuarios, roles y una tabla intermedia para soportar relaciones de muchos a muchos (*Many-to-Many*), permitiendo que un usuario posea múltiples roles.

### 1. Diagrama Conceptual (ASCII)

```text
+-------------------+           +-----------------------+           +-------------------+
|      Users        |           |      UserRoles        |           |       Roles       |
+-------------------+           +-----------------------+           +-------------------+
| PK  Id (INT)      |<----+     | PK,FK UserId (INT)    |     +---->| PK  Id (INT)      |
|     Username      |     |     | PK,FK RoleId (INT)    |     |     |     Name          |
|     Email         |     +-----<(Relación N:M)---------+-----+     |     Description   |
|     PasswordHash  |           +-----------------------+           +-------------------+
|     CreateAt      |
+-------------------+
```

### 2. Descripción de Tablas

* **`Users`**: Almacena la información de las cuentas de usuario registradas.
  * `Id` (PK, Int, Auto-incremental)
  * `Username` (Varchar, Unique)
  * `Email` (Varchar, Unique)
  * `PasswordHash` (Varchar - Hashed con BCrypt)
  * `CreateAt` (DateTime)

* **`Roles`**: Define los niveles de acceso del sistema (ej. `SuperAdmin`, `Admin`, `User`).
  * `Id` (PK, Int, Auto-incremental)
  * `Name` (Varchar, Unique)

* **`UserRoles` (Tabla Pivote)**: Resuelve la relación de muchos a muchos entre usuarios y roles.
  * `UserId` (PK, FK hacia `Users.Id`)
  * `RoleId` (PK, FK hacia `Roles.Id`)

---

## 🔌 Endpoints Principales

| Método | Endpoint | Descripción | Acceso |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/login` | Inicia sesión y devuelve un token JWT. | Público (`AllowAnonymous`) |
| `POST` | `/api/users/register` | Registra un nuevo usuario en el sistema. | `SuperAdmin` |
| `GET` | `/api/users` | Lista todos los usuarios con sus roles asociados. | `SuperAdmin`, `Admin` |

---

## 🛡️ Contribución y Autor
Desarrollado como parte de un ecosistema de microservicios y APIs REST seguras en .NET.