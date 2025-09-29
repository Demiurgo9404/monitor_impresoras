using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Tests.Unit
{
    public class PermissionServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly PermissionService _permissionService;

        public PermissionServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Setup audit service mock
            _auditServiceMock = new Mock<IAuditService>();

            // Setup PermissionService
            _permissionService = new PermissionService(_context, _auditServiceMock.Object, Mock.Of<ILogger<PermissionService>>());
        }

        [Fact]
        public async Task GetUserClaimsAsync_WithValidUser_ShouldReturnClaims()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var claim = new UserClaim
            {
                Id = 1,
                UserId = user.Id,
                ClaimType = "printers.manage",
                ClaimValue = "true",
                Description = "Can manage printers",
                Category = "printers",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _context.UserClaims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.GetUserClaimsAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            var claimDto = result.First();
            claimDto.ClaimType.Should().Be("printers.manage");
            claimDto.ClaimValue.Should().Be("true");
            claimDto.Category.Should().Be("printers");
        }

        [Fact]
        public async Task GetUserClaimsAsync_WithNonExistentUser_ShouldReturnEmpty()
        {
            // Act
            var result = await _permissionService.GetUserClaimsAsync("non-existent-user");

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AssignClaimToUserAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.AssignClaimToUserAsync(
                user.Id, "reports.view", "true", "Can view reports", "reports", null, "admin-user");

            // Assert
            result.Should().BeTrue();

            var savedClaim = await _context.UserClaims.FirstOrDefaultAsync();
            savedClaim.Should().NotBeNull();
            savedClaim!.ClaimType.Should().Be("reports.view");
            savedClaim.ClaimValue.Should().Be("true");
            savedClaim.Description.Should().Be("Can view reports");
            savedClaim.Category.Should().Be("reports");
        }

        [Fact]
        public async Task AssignClaimToUserAsync_WithExistingClaim_ShouldUpdateClaim()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var existingClaim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = "printers.manage",
                ClaimValue = "false",
                IsActive = true
            };
            await _context.UserClaims.AddAsync(existingClaim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.AssignClaimToUserAsync(
                user.Id, "printers.manage", "true", "Updated description", "printers", null, "admin-user");

            // Assert
            result.Should().BeTrue();

            var updatedClaim = await _context.UserClaims.FirstOrDefaultAsync();
            updatedClaim.Should().NotBeNull();
            updatedClaim!.ClaimValue.Should().Be("true");
            updatedClaim.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task RevokeClaimFromUserAsync_WithValidClaim_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var claim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = "reports.view",
                ClaimValue = "true",
                IsActive = true
            };
            await _context.UserClaims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.RevokeClaimFromUserAsync(user.Id, "reports.view", "admin-user");

            // Assert
            result.Should().BeTrue();

            var revokedClaim = await _context.UserClaims.FirstOrDefaultAsync();
            revokedClaim.Should().NotBeNull();
            revokedClaim!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task RevokeClaimFromUserAsync_WithNonExistentClaim_ShouldReturnFalse()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.RevokeClaimFromUserAsync(user.Id, "non.existent.claim", "admin-user");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UserHasClaimAsync_WithValidClaim_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var claim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = "printers.manage",
                ClaimValue = "true",
                IsActive = true,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(1)
            };
            await _context.UserClaims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.UserHasClaimAsync(user.Id, "printers.manage", "true");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserHasClaimAsync_WithExpiredClaim_ShouldReturnFalse()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var claim = new UserClaim
            {
                UserId = user.Id,
                ClaimType = "printers.manage",
                ClaimValue = "true",
                IsActive = true,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1) // Expired
            };
            await _context.UserClaims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.UserHasClaimAsync(user.Id, "printers.manage", "true");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAvailableClaimsAsync_ShouldReturnSystemClaims()
        {
            // Act
            var result = await _permissionService.GetAvailableClaimsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThan(0);

            var claimsList = result.ToList();
            claimsList.Should().Contain(c => c.ClaimType == "printers.manage");
            claimsList.Should().Contain(c => c.ClaimType == "reports.view");
            claimsList.Should().Contain(c => c.ClaimType == "users.manage");
        }

        [Fact]
        public async Task GetUserClaimTypesAsync_ShouldReturnDistinctTypes()
        {
            // Arrange
            var user = new User { Id = "test-user-id", UserName = "testuser" };
            await _context.Users.AddAsync(user);

            var claims = new List<UserClaim>
            {
                new UserClaim { UserId = user.Id, ClaimType = "printers.manage", ClaimValue = "true", IsActive = true },
                new UserClaim { UserId = user.Id, ClaimType = "reports.view", ClaimValue = "true", IsActive = true },
                new UserClaim { UserId = user.Id, ClaimType = "printers.manage", ClaimValue = "false", IsActive = true }
            };

            await _context.UserClaims.AddRangeAsync(claims);
            await _context.SaveChangesAsync();

            // Act
            var result = await _permissionService.GetUserClaimTypesAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // Should be distinct
            result.Should().Contain("printers.manage");
            result.Should().Contain("reports.view");
        }
    }
}
