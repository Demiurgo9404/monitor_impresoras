using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Services.MultiTenancy;
using Xunit;

namespace QOPIQ.Tests.Services.MultiTenancy
{
    public class TenantResolverTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ITenantAccessor> _mockTenantAccessor;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<ILogger<TenantResolver>> _mockLogger;
        private readonly TenantResolver _resolver;

        public TenantResolverTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockTenantAccessor = new Mock<ITenantAccessor>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockLogger = new Mock<ILogger<TenantResolver>>();

            _resolver = new TenantResolver(
                _mockHttpContextAccessor.Object,
                _mockTenantAccessor.Object,
                _mockConfiguration.Object,
                _mockDbContext.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task ResolveTenantIdentifierAsync_ShouldReturnExistingTenant_WhenAlreadySet()
        {
            // Arrange
            var expectedTenantId = "test-tenant";
            _mockTenantAccessor.Setup(x => x.TenantId).Returns(expectedTenantId);

            // Act
            var result = await _resolver.ResolveTenantIdentifierAsync();

            // Assert
            Assert.Equal(expectedTenantId, result);
            _mockTenantAccessor.Verify(x => x.TenantId, Times.Once);
        }

        [Fact]
        public async Task ResolveTenantIdentifierAsync_ShouldReturnTenantFromHeader_WhenHeaderExists()
        {
            // Arrange
            var expectedTenantId = "header-tenant";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Tenant-Id"] = expectedTenantId;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Mock database setup
            var mockDbSet = TestHelpers.MockDbSet(new List<Tenant>
            {
                new Tenant { Id = expectedTenantId, Name = "Test Tenant" }
            }.AsQueryable());
            
            _mockDbContext.Setup(x => x.Tenants).Returns(mockDbSet.Object);

            // Act
            var result = await _resolver.ResolveTenantIdentifierAsync();

            // Assert
            Assert.Equal(expectedTenantId, result);
            _mockTenantAccessor.Verify(x => x.SetTenant(expectedTenantId), Times.Once);
        }

        [Fact]
        public async Task ResolveTenantIdentifierAsync_ShouldReturnDefaultTenant_WhenNoOtherSourceAvailable()
        {
            // Arrange
            var defaultTenantId = "default-tenant";
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            
            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.Value).Returns(defaultTenantId);
            _mockConfiguration.Setup(x => x.GetSection("MultiTenancy:DefaultTenantId"))
                .Returns(configSection.Object);

            // Act
            var result = await _resolver.ResolveTenantIdentifierAsync();

            // Assert
            Assert.Equal(defaultTenantId, result);
            _mockTenantAccessor.Verify(x => x.SetTenant(defaultTenantId), Times.Once);
        }
    }
}
