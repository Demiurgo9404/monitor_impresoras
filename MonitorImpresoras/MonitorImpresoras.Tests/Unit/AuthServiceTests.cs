using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Infrastructure.Services;

namespace MonitorImpresoras.Tests.Unit
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup configuration
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = "TestKey123456789012345678901234567890",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["Jwt:AccessTokenMinutes"] = "15",
                    ["Jwt:RefreshTokenDays"] = "7"
                });
            _configuration = configBuilder.Build();

            // Setup mocks
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);

            _tokenServiceMock = new Mock<ITokenService>();

            // Setup AuthService
            _authService = new AuthService(
                _userManagerMock.Object,
                _tokenServiceMock.Object,
                _context,
                Mock.Of<ILogger<AuthService>>());
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponseDto()
        {
            // Arrange
            var user = new User
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };

            var roles = new List<string> { "Admin" };
            var accessToken = "test-access-token";
            var refreshToken = "test-refresh-token";

            _userManagerMock.Setup(um => um.FindByNameAsync("testuser"))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, "password123"))
                .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(user, roles))
                .Returns(accessToken);
            _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
                .Returns(refreshToken);

            var loginDto = new LoginRequestDto
            {
                Username = "testuser",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginDto, "192.168.1.1", "TestAgent");

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(accessToken);
            result.RefreshToken.Should().Be(refreshToken);
            result.Roles.Should().BeEquivalentTo(roles);
            result.UserId.Should().Be(user.Id);

            // Verify refresh token was saved to database
            var savedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync();
            savedRefreshToken.Should().NotBeNull();
            savedRefreshToken!.Token.Should().Be(refreshToken);
            savedRefreshToken.UserId.Should().Be(user.Id);
            savedRefreshToken.CreatedByIp.Should().Be("192.168.1.1");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var user = new User
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };

            _userManagerMock.Setup(um => um.FindByNameAsync("testuser"))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, "wrongpassword"))
                .ReturnsAsync(false);

            var loginDto = new LoginRequestDto
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.LoginAsync(loginDto, "192.168.1.1", "TestAgent"));
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange
            var user = new User
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };

            var roles = new List<string> { "Admin" };
            var oldRefreshToken = "old-refresh-token";
            var newAccessToken = "new-access-token";
            var newRefreshToken = "new-refresh-token";

            // Create existing refresh token
            var existingToken = new RefreshToken
            {
                Id = 1,
                Token = oldRefreshToken,
                UserId = user.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByIp = "192.168.1.1",
                Revoked = false
            };

            await _context.RefreshTokens.AddAsync(existingToken);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(um => um.FindByIdAsync(user.Id))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(user, roles))
                .Returns(newAccessToken);
            _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
                .Returns(newRefreshToken);

            // Act
            var result = await _authService.RefreshTokenAsync(oldRefreshToken, "192.168.1.1");

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(newAccessToken);
            result.RefreshToken.Should().Be(newRefreshToken);

            // Verify old token was revoked and new one created
            var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == oldRefreshToken);
            revokedToken.Should().NotBeNull();
            revokedToken!.Revoked.Should().BeTrue();
            revokedToken.ReplacedByToken.Should().Be(newRefreshToken);

            var newToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == newRefreshToken);
            newToken.Should().NotBeNull();
            newToken!.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ShouldReturnNull()
        {
            // Act
            var result = await _authService.RefreshTokenAsync("invalid-token", "192.168.1.1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var expiredToken = new RefreshToken
            {
                Id = 1,
                Token = "expired-token",
                UserId = "test-user-id",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1), // Expired
                CreatedAtUtc = DateTime.UtcNow,
                Revoked = false
            };

            await _context.RefreshTokens.AddAsync(expiredToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.RefreshTokenAsync("expired-token", "192.168.1.1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Id = 1,
                Token = "valid-token",
                UserId = "test-user-id",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
                CreatedAtUtc = DateTime.UtcNow,
                Revoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.LogoutAsync("valid-token", "192.168.1.1");

            // Assert
            result.Should().BeTrue();

            var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync();
            revokedToken.Should().NotBeNull();
            revokedToken!.Revoked.Should().BeTrue();
            revokedToken.RevokedAtUtc.Should().NotBeNull();
            revokedToken.RevokedByIp.Should().Be("192.168.1.1");
        }

        [Fact]
        public async Task LogoutAsync_WithInvalidToken_ShouldReturnFalse()
        {
            // Act
            var result = await _authService.LogoutAsync("invalid-token", "192.168.1.1");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var user = new User();
            var registerDto = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User"
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _authService.RegisterAsync(registerDto);

            // Assert
            _userManagerMock.Verify(um => um.CreateAsync(
                It.Is<User>(u => u.UserName == registerDto.Username && u.Email == registerDto.Email),
                registerDto.Password), Times.Once);
            _userManagerMock.Verify(um => um.AddToRoleAsync(
                It.IsAny<User>(), "User"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterRequestDto
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "Password123!"
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already exists" }));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() =>
                _authService.RegisterAsync(registerDto));
        }
    }
}
