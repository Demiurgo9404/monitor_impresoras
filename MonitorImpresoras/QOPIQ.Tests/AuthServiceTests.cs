using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.DTOs;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Services;
using Moq;
using Xunit;

namespace QOPIQ.Tests
{
    public class AuthServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;
        private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
        private readonly Mock<ITenantAccessor> _tenantAccessorMock;

        public AuthServiceTests()
        {
            // Configurar base de datos en memoria
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _tenantAccessorMock = new Mock<ITenantAccessor>();
            _context = new ApplicationDbContext(options, _tenantAccessorMock.Object);

            // Configurar mocks
            _passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var loggerMock = new Mock<ILogger<AuthService>>();
            var jwtLoggerMock = new Mock<ILogger<JwtService>>();

            // Configurar JWT Service
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = "test-key-for-unit-tests-must-be-at-least-32-characters-long",
                    ["Jwt:Issuer"] = "QopiqTestAPI",
                    ["Jwt:Audience"] = "QopiqTestClient",
                    ["Jwt:ExpirationInMinutes"] = "60"
                })
                .Build();

            _jwtService = new JwtService(configuration, jwtLoggerMock.Object);
            _authService = new AuthService(_context, _jwtService, _passwordHasherMock.Object, loggerMock.Object);

            // Seed datos de prueba
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                TenantKey = "test-tenant",
                Name = "Test Tenant",
                CompanyName = "Test Company",
                AdminEmail = "admin@test.com",
                IsActive = true,
                Tier = SubscriptionTier.Professional,
                MaxUsers = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var company = new Company
            {
                Id = Guid.NewGuid(),
                TenantId = "test-tenant",
                Name = "Test Company",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "test-tenant",
                CompanyId = company.Id,
                Email = "test@test.com",
                UserName = "test@test.com",
                FirstName = "Test",
                LastName = "User",
                Role = QopiqRoles.CompanyAdmin,
                IsActive = true,
                EmailConfirmed = true,
                PasswordHash = "hashed-password",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            _context.Companies.Add(company);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@test.com",
                Password = "password123"
            };

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), "hashed-password", "password123"))
                .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _authService.LoginAsync(loginDto, "test-tenant");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.NotEmpty(result.RefreshToken);
            Assert.Equal("test@test.com", result.User.Email);
            Assert.Equal("test-tenant", result.Tenant.TenantId);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@test.com",
                Password = "wrong-password"
            };

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), "hashed-password", "wrong-password"))
                .Returns(PasswordVerificationResult.Failed);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto, "test-tenant"));
        }

        [Fact]
        public async Task LoginAsync_WithInvalidTenant_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@test.com",
                Password = "password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginDto, "invalid-tenant"));
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "newuser@test.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User",
                Role = QopiqRoles.Viewer
            };

            _passwordHasherMock
                .Setup(x => x.HashPassword(It.IsAny<User>(), "password123"))
                .Returns("hashed-new-password");

            _passwordHasherMock
                .Setup(x => x.VerifyHashedPassword(It.IsAny<User>(), "hashed-new-password", "password123"))
                .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _authService.RegisterAsync(registerDto, "test-tenant");

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Token);
            Assert.Equal("newuser@test.com", result.User.Email);
            Assert.Equal(QopiqRoles.Viewer, result.User.Role);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Email = "test@test.com", // Email ya existe
                Password = "password123",
                FirstName = "Duplicate",
                LastName = "User"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _authService.RegisterAsync(registerDto, "test-tenant"));
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var user = await _context.Users.FirstAsync(u => u.Email == "test@test.com");
            var permissions = QopiqPermissions.RolePermissions[QopiqRoles.CompanyAdmin];
            var token = _jwtService.GenerateToken(user, "test-tenant", permissions);

            // Act
            var result = await _authService.ValidateTokenAsync(token, "test-tenant");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnFalse()
        {
            // Arrange
            var invalidToken = "invalid.jwt.token";

            // Act
            var result = await _authService.ValidateTokenAsync(invalidToken, "test-tenant");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void JwtService_GenerateToken_ShouldCreateValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "jwt@test.com",
                FirstName = "JWT",
                LastName = "Test",
                Role = QopiqRoles.Viewer
            };
            var permissions = new[] { "read:printers" };

            // Act
            var token = _jwtService.GenerateToken(user, "test-tenant", permissions);

            // Assert
            Assert.NotEmpty(token);
            Assert.Contains(".", token); // JWT debe tener puntos separadores

            // Validar que se puede leer
            var claims = _jwtService.GetClaimsFromToken(token);
            Assert.NotNull(claims);

            var tenantId = claims.FindFirst(QopiqClaims.TenantId)?.Value;
            Assert.Equal("test-tenant", tenantId);

            var userId = claims.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Assert.Equal(user.Id, userId);
        }

        [Fact]
        public void JwtService_ValidateToken_WithValidToken_ShouldReturnClaims()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = "validate@test.com",
                FirstName = "Validate",
                LastName = "Test"
            };
            var token = _jwtService.GenerateToken(user, "test-tenant", Array.Empty<string>());

            // Act
            var claims = _jwtService.ValidateToken(token);

            // Assert
            Assert.NotNull(claims);
            Assert.True(claims.Identity?.IsAuthenticated);
        }

        [Fact]
        public void JwtService_ValidateToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var claims = _jwtService.ValidateToken(invalidToken);

            // Assert
            Assert.Null(claims);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

