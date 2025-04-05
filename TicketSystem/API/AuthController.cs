using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Necesario para leer appsettings
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens; // Necesario para SymmetricSecurityKey etc.
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // Necesario para JwtSecurityTokenHandler
using System.Security.Claims; // Necesario para Claims
using System.Text; // Necesario para Encoding
using System.Threading.Tasks;
using TicketSystem.Models; // Necesario para User y LoginRequestDto

namespace TicketSystem.API // Namespace de la carpeta API
{
    [Route("api/[controller]")] // Ruta base: /api/Auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager; // Útil para CheckPasswordSignInAsync aunque no iniciemos sesión con cookie
        private readonly IConfiguration _configuration; // Para leer appsettings (Jwt:Key, etc.)
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Inicia sesión para un usuario y devuelve un token JWT si las credenciales son válidas.
        /// </summary>
        /// <param name="loginRequest">Objeto con Email y Password.</param>
        /// <returns>Un objeto con el token JWT o un error de autorización/servidor.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Devuelve el token
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Si el DTO es inválido
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Si las credenciales son inválidas
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            // [ApiController] valida ModelState del loginRequest automáticamente.

            _logger.LogInformation("Intento de login API para {Email}", loginRequest.Email);

            try
            {
                // 1. Buscar al usuario por Email
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login API fallido: Usuario no encontrado para {Email}", loginRequest.Email);
                    return Unauthorized(new { message = "Credenciales inválidas." }); // 401
                }

                // 2. Verificar la contraseña (sin iniciar sesión con cookie)
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login API fallido: Contraseña inválida para {Email}", loginRequest.Email);
                    // Podrías querer loguear el motivo específico si result tiene más detalles (ej. IsLockedOut)
                    return Unauthorized(new { message = "Credenciales inválidas." }); // 401
                }

                // 2.5 (Opcional pero recomendado) Verificar si el email está confirmado si lo requieres
                // if (_userManager.Options.SignIn.RequireConfirmedAccount && !await _userManager.IsEmailConfirmedAsync(user))
                // {
                //     _logger.LogWarning("Login API fallido: Email no confirmado para {Email}", loginRequest.Email);
                //     return Unauthorized(new { message = "Debe confirmar su correo electrónico para iniciar sesión." }); // 401
                // }


                // 3. Si las credenciales son válidas, generar el Token JWT
                _logger.LogInformation("Credenciales válidas para {Email}. Generando token JWT...", loginRequest.Email);

                var jwtKey = _configuration["Jwt:Key"];
                var jwtIssuer = _configuration["Jwt:Issuer"];
                var jwtAudience = _configuration["Jwt:Audience"];

                if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
                {
                    _logger.LogError("Configuración JWT (Key, Issuer, Audience) incompleta en appsettings.json");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error interno de configuración del servidor.");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Crear los 'Claims' (información que irá dentro del token)
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id), // ID del usuario (estándar 'subject')
                    new Claim(JwtRegisteredClaimNames.Email, user.Email), // Email del usuario (estándar)
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único para el token (estándar 'jwt id')
                    // --- Claims Personalizados ---
                    new Claim("uid", user.Id), // Repetir ID si se necesita con otro nombre de claim
                    new Claim(ClaimTypes.Role, user.Role.ToString()) // Añadir nuestro rol personalizado como Claim de Rol estándar
                    // Podrías añadir más claims si los necesitas (ej. nombre, etc.)
                };

                // Añadir roles de Identity si los estuvieras usando de forma principal
                // var identityRoles = await _userManager.GetRolesAsync(user);
                // foreach (var role in identityRoles) { claims.Add(new Claim(ClaimTypes.Role, role)); }


                // Definir el token
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1), // Tiempo de expiración del token (ej. 1 hora) - ¡Ajusta según necesidad!
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
                    SigningCredentials = credentials
                };

                // Crear y escribir el token
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Token JWT generado exitosamente para {Email}", loginRequest.Email);

                // 4. Devolver el token al cliente
                return Ok(new { token = tokenString }); // Devuelve 200 OK con el token
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el login API para {Email}", loginRequest.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error interno al intentar iniciar sesión.");
            }
        }
    }
}
