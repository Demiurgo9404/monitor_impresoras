using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Infrastructure.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userManagerMock = MockUserManager();
            _signInManagerMock = MockSignInManager();
            _tokenServiceMock = new Mock<ITokenService>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnTrue_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registerDto = new RegisterRequestDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().BeTrue();
            _userManagerMock.Verify(um => um.CreateAsync(It.Is<User>(u =>
                u.UserName == registerDto.Username &&
                u.Email == registerDto.Email &&
                u.FirstName == registerDto.FirstName &&
                u.LastName == registerDto.LastName), registerDto.Password), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFalse_WhenRegistrationFails()
        {
            // Arrange
            var registerDto = new RegisterRequestDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var identityErrors = new List<IdentityError>
            {
                new IdentityError { Description = "Username already exists" }
            };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnLoginResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "testuser",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = "123",
                UserName = "testuser",
                Email = "test@example.com",
                IsActive = true
            };

            var expectedToken = new LoginResponseDto
            {
                Token = "jwt-token",
                RefreshToken = "refresh-token",
                Expiration = DateTime.UtcNow.AddHours(1),
                Roles = new List<string> { "User" }
            };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            _userManagerMock.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(user, It.IsAny<List<string>>()))
                .Returns("jwt-token");

            _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
                .Returns("refresh-token");

            _userManagerMock.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.LoginAsync(loginDto, "127.0.0.1", "Test Agent");

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be("jwt-token");
            result.RefreshToken.Should().Be("refresh-token");
            result.Roles.Should().Contain("User");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "nonexistent",
                Password = "Password123!"
            };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginDto.Username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync(loginDto, "127.0.0.1", "Test Agent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "testuser",
                Password = "WrongPassword!"
            };

            var user = new User
            {
                Id = "123",
                UserName = "testuser",
                Email = "test@example.com",
                IsActive = true
            };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginDto.Username))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginAsync(loginDto, "127.0.0.1", "Test Agent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var refreshDto = new RefreshTokenRequestDto
            {
                RefreshToken = "valid-refresh-token"
            };

            var user = new User
            {
                Id = "123",
                UserName = "testuser",
                RefreshToken = "valid-refresh-token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1)
            };

            var expectedResponse = new LoginResponseDto
            {
                Token = "new-jwt-token",
                RefreshToken = "new-refresh-token",
                Expiration = DateTime.UtcNow.AddHours(1),
                Roles = new List<string> { "User" }
            };

            _userManagerMock.Setup(um => um.Users)
                .Returns(new List<User> { user }.AsQueryable());

            _tokenServiceMock.Setup(ts => ts.GenerateAccessToken(user, It.IsAny<List<string>>()))
                .Returns("new-jwt-token");

            _tokenServiceMock.Setup(ts => ts.GenerateRefreshToken())
                .Returns("new-refresh-token");

            _userManagerMock.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshDto, "127.0.0.1");

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be("new-jwt-token");
            result.RefreshToken.Should().Be("new-refresh-token");
        }

        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNull_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var refreshDto = new RefreshTokenRequestDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            var user = new User
            {
                Id = "123",
                UserName = "testuser",
                RefreshToken = "valid-refresh-token", // Different from requested
                RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(1)
            };

            _userManagerMock.Setup(um => um.Users)
                .Returns(new List<User> { user }.AsQueryable());

            // Act
            var result = await _authService.RefreshTokenAsync(refreshDto, "127.0.0.1");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnTrue_WhenLogoutIsSuccessful()
        {
            // Arrange
            var userId = "123";
            var user = new User { Id = userId };

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync(user);

            _userManagerMock.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.LogoutAsync(userId);

            // Assert
            result.Should().BeTrue();
            user.RefreshToken.Should().BeNull();
            user.RefreshTokenExpiryTime.Should().BeNull();
        }

        [Fact]
        public async Task LogoutAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var userId = "nonexistent";

            _userManagerMock.Setup(um => um.FindByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LogoutAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        private Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var userManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.Users)
                .Returns(Enumerable.Empty<User>().AsQueryable());

            return userManager;
        }

        private Mock<SignInManager<User>> MockSignInManager()
        {
            var userManager = MockUserManager();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

            return new Mock<SignInManager<User>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);
        }
    }
}
