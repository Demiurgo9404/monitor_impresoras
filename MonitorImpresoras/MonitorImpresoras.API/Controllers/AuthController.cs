using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MonitorImpresoras.API.Models;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<User> userManager,
            IOptions<JwtSettings> jwtSettings,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Asignar rol por defecto
            await _userManager.AddToRoleAsync(user, "User");

            // Si es el primer usuario, hacerlo administrador
            if (_userManager.Users.Count() == 1)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model, [FromQuery] bool rememberMe = false)
        {
            _logger.LogInformation("Intento de inicio de sesión para el usuario: {Username}", model.Username);
            
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Datos de inicio de sesión no válidos" });

            // Prevención de ataques de fuerza bruta
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP desconocida";
            var cacheKey = $"login_attempts_{ipAddress}_{model.Username}";
            var attempts = await _context.LoginAttempts
                .Where(x => x.Username == model.Username && x.IpAddress == ipAddress && x.AttemptDate > DateTime.UtcNow.AddMinutes(-15))
                .CountAsync();

            if (attempts >= 5)
            {
                _logger.LogWarning("Demasiados intentos de inicio de sesión para el usuario: {Username} desde la IP: {IpAddress}", 
                    model.Username, ipAddress);
                return StatusCode(429, new { message = "Demasiados intentos. Por favor, espere 15 minutos." });
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Registrar intento fallido
                await _context.LoginAttempts.AddAsync(new LoginAttempt
                {
                    Username = model.Username,
                    IpAddress = ipAddress,
                    AttemptDate = DateTime.UtcNow,
                    Success = false
                });
                await _context.SaveChangesAsync();

                _logger.LogWarning("Intento de inicio de sesión fallido para el usuario: {Username}", model.Username);
                return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Intento de inicio de sesión para usuario inactivo: {Username}", model.Username);
                return Unauthorized(new { message = "La cuenta de usuario está desactivada" });
            }

            // Autenticación exitosa
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Registrar inicio de sesión exitoso
            await _context.LoginAttempts.AddAsync(new LoginAttempt
            {
                Username = model.Username,
                IpAddress = ipAddress,
                AttemptDate = DateTime.UtcNow,
                Success = true
            });
            await _context.SaveChangesAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, userRoles);
            var refreshToken = GenerateRefreshToken();

            // Guardar el refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(rememberMe ? 30 : 7); // 30 días si recuérdame está activo, 7 días si no
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Inicio de sesión exitoso para el usuario: {Username}", model.Username);

            return Ok(new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = userRoles.ToList(),
                    FullName = $"{user.FirstName} {user.LastName}".Trim()
                }
            });
        }

        private JwtSecurityToken GenerateJwtToken(User user, IList<string> roles)
        {
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("fullName", $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Agregar roles como claims
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Agregar claims personalizados adicionales si es necesario
            authClaims.Add(new Claim("ipAddress", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(
                    authSigningKey, 
                    SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            if (string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new { message = "Token y refresh token son requeridos" });
            }

            var principal = GetPrincipalFromExpiredToken(model.Token);
            if (principal == null)
            {
                return BadRequest(new { message = "Token inválido" });
            }

            var username = principal.Identity?.Name;
            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "Refresh token inválido o expirado" });
            }

            var newJwtToken = GenerateJwtToken(user, await _userManager.GetRolesAsync(user));
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Renovar por 7 días más
            await _userManager.UpdateAsync(user);

            return Ok(new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(newJwtToken),
                RefreshToken = newRefreshToken,
                Expiration = newJwtToken.ValidTo,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = await _userManager.GetRolesAsync(user) as List<string>,
                    FullName = $"{user.FirstName} {user.LastName}".Trim()
                }
            });
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false, // Importante: no validar la expiración aquí
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token inválido");
            }

            return principal;
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenModel model)
        {
            var username = User.Identity?.Name;
            var user = await _userManager.FindByNameAsync(username);
            
            if (user == null) return BadRequest(new { message = "Usuario no encontrado" });
            
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
            
            return Ok(new { message = "Token revocado exitosamente" });
        }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Solo se permiten letras, números y guiones bajos")]
        public string Username { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico no válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "La contraseña debe contener al menos una letra mayúscula, una minúscula, un número y un carácter especial")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string LastName { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
    }

    public class RefreshTokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RevokeTokenModel
    {
        public string RefreshToken { get; set; }
    }
}
