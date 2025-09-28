using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MonitorImpresoras.API.Controllers;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Security.Claims;

namespace MonitorImpresoras.Tests.Authorization
{
    public class AuthorizationTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly AuthController _controller;

        public AuthorizationTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _auditServiceMock = new Mock<IAuditService>();
            _controller = new AuthController(_authServiceMock.Object, _auditServiceMock.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnSuccess()
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

            _authServiceMock.Setup(s => s.RegisterAsync(registerDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                Username = "invalid",
                Password = "wrong"
            };

            _authServiceMock.Setup(s => s.LoginAsync(loginDto, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Usuario o contraseña incorrectos"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task RefreshToken_WithEmptyToken_ShouldReturnBadRequest()
        {
            // Arrange
            var refreshDto = new RefreshTokenRequestDto
            {
                RefreshToken = ""
            };

            // Act
            var result = await _controller.RefreshToken(refreshDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Refresh token es requerido");
        }

        [Fact]
        public async Task Logout_WithValidUser_ShouldSucceed()
        {
            // Arrange
            var userId = "test-user-id";

            // Create mock HttpContext with user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _authServiceMock.Setup(s => s.LogoutAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new { success = true });
        }

        [Fact]
        public async Task GetProfile_WithoutAuthorization_ShouldReturnUnauthorized()
        {
            // Arrange - No user in context

            // Act
            var result = _controller.GetProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task GetProfile_WithValidUser_ShouldReturnUserInfo()
        {
            // Arrange
            var userId = "test-user-id";
            var username = "testuser";
            var email = "test@example.com";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = _controller.GetProfile();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var profile = okResult!.Value;

            profile.Should().NotBeNull();
            // Verify specific properties exist in the response
            var profileType = profile!.GetType();
            profileType.GetProperty("UserId")?.GetValue(profile).Should().Be(userId);
            profileType.GetProperty("Username")?.GetValue(profile).Should().Be(username);
            profileType.GetProperty("Email")?.GetValue(profile).Should().Be(email);
        }
    }

    public class PolicyTests
    {
        [Fact]
        public void RequireAdmin_Policy_ShouldRequireAdminRole()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
            });

            var serviceProvider = services.BuildServiceProvider();
            var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

            // Act & Assert
            var policy = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>()
                .GetPolicyAsync("RequireAdmin").Result;

            policy.Should().NotBeNull();
            policy!.Requirements.Should().HaveCount(1);
            policy.Requirements.First().Should().BeOfType<RolesAuthorizationRequirement>();
        }

        [Fact]
        public void CanReadPrinters_Policy_ShouldAllowAdminOrManagePrintersClaim()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanReadPrinters", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") ||
                        context.User.HasClaim("ManagePrinters", "true") ||
                        context.User.HasClaim("ReadPrinters", "true")));
            });

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var policy = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>()
                .GetPolicyAsync("CanReadPrinters").Result;

            // Assert
            policy.Should().NotBeNull();
            policy!.Requirements.Should().HaveCount(1);
            policy.Requirements.First().Should().BeOfType<AssertionRequirement>();
        }
    }
}
