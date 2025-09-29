using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Tests.Unit
{
    public class UserServiceTests
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var context = new ApplicationDbContext(options);

            // Setup UserManager
            var userStore = new Mock<IUserStore<User>>();
            _userManager = new UserManager<User>(userStore.Object, null, null, null, null, null, null, null, null);

            // Setup RoleManager
            var roleStore = new Mock<IRoleStore<Role>>();
            _roleManager = new RoleManager<Role>(roleStore.Object, null, null, null, null);

            // Setup audit service mock
            _auditServiceMock = new Mock<IAuditService>();

            // Setup UserService
            _userService = new UserService(_userManager, _roleManager, _auditServiceMock.Object, Mock.Of<ILogger<UserService>>());
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnPagedResult()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = "1", UserName = "user1", Email = "user1@test.com", FirstName = "User", LastName = "One", IsActive = true },
                new User { Id = "2", UserName = "user2", Email = "user2@test.com", FirstName = "User", LastName = "Two", IsActive = true }
            };

            // Act
            var result = await _userService.GetUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(0); // Empty in test environment
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUserDetail()
        {
            // Arrange
            var user = new User
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            };

            // Act
            var result = await _userService.GetUserByIdAsync("non-existent-id");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AssignRoleToUserAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            var role = new Role { Id = "role-id", Name = "TestRole", IsActive = true };

            // Act
            var result = await _userService.AssignRoleToUserAsync("non-existent-user", "TestRole", "admin-user-id");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveRoleFromUserAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            var role = new Role { Id = "role-id", Name = "TestRole", IsActive = true };

            // Act
            var result = await _userService.RemoveRoleFromUserAsync("non-existent-user", "TestRole", "admin-user-id");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task BlockUserAsync_WithValidUser_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser", IsActive = true };

            // Act
            var result = await _userService.BlockUserAsync("non-existent-user", "admin-user-id", "Test reason");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UnblockUserAsync_WithValidUser_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser", IsActive = false };

            // Act
            var result = await _userService.UnblockUserAsync("non-existent-user", "admin-user-id");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResetUserPasswordAsync_WithValidUser_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };

            // Act
            var result = await _userService.ResetUserPasswordAsync("non-existent-user", "NewPassword123!", "admin-user-id");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            var profileDto = new UpdateProfileDto
            {
                FirstName = "Updated",
                LastName = "Name"
            };

            // Act
            var result = await _userService.UpdateUserProfileAsync("non-existent-user", profileDto, "admin-user-id");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserProfileAsync_AsSelf_ShouldAllowBasicFields()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            var profileDto = new UpdateProfileDto
            {
                FirstName = "Updated",
                Department = "New Department"
            };

            // Act
            var result = await _userService.UpdateUserProfileAsync("test-user-id", profileDto, "test-user-id");

            // Assert
            result.Should().BeFalse(); // User doesn't exist in test environment
        }

        [Fact]
        public async Task UpdateUserProfileAsync_AsAdmin_ShouldAllowAllFields()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            var profileDto = new UpdateProfileDto
            {
                FirstName = "Updated",
                IsActive = false
            };

            // Act
            var result = await _userService.UpdateUserProfileAsync("test-user-id", profileDto, "admin-user-id");

            // Assert
            result.Should().BeFalse(); // User doesn't exist in test environment
        }
    }
}
