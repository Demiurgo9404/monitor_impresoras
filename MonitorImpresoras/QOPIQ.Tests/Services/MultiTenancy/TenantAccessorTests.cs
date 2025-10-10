using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QOPIQ.Application.Interfaces.MultiTenancy;
using QOPIQ.Infrastructure;
using Xunit;

namespace QOPIQ.Tests.Services.MultiTenancy
{
    public class TenantAccessorTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ILogger<TenantAccessor>> _mockLogger;
        private readonly TenantAccessor _accessor;

        public TenantAccessorTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogger = new Mock<ILogger<TenantAccessor>>();
            _accessor = new TenantAccessor(_mockHttpContextAccessor.Object, _mockLogger.Object);
        }

        [Fact]
        public void TenantId_ShouldReturnNull_WhenNoTenantSet()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _accessor.TenantId;

            // Assert
            Assert.Null(result);
            Assert.False(_accessor.HasTenant);
        }

        [Fact]
        public void SetTenant_ShouldSetTenantId_WhenValidTenantIdProvided()
        {
            // Arrange
            var tenantId = "test-tenant";

            // Act
            _accessor.SetTenant(tenantId);

            // Assert
            Assert.Equal(tenantId, _accessor.TenantId);
            Assert.True(_accessor.HasTenant);
        }

        [Fact]
        public void SetTenant_ShouldThrow_WhenNullTenantIdProvided()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _accessor.SetTenant(null!));
            Assert.Equal("tenantId", exception.ParamName);
        }

        [Fact]
        public void SetTenant_ShouldBeThreadSafe_WhenCalledFromMultipleThreads()
        {
            // Arrange
            const int threadCount = 10;
            var tenantIds = new string[threadCount];
            var threads = new Thread[threadCount];

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                var index = i;
                threads[i] = new Thread(() =>
                {
                    var tenantId = $"tenant-{index}";
                    _accessor.SetTenant(tenantId);
                    tenantIds[index] = _accessor.TenantId;
                    Thread.Sleep(10); // Ensure threads overlap
                });
                threads[i].Start();
            }

            // Wait for all threads to complete
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Assert
            // Only one thread should have successfully set the tenant ID
            var nonNullTenantIds = tenantIds.Where(id => id != null).Distinct().ToList();
            Assert.Single(nonNullTenantIds);
        }

        [Fact]
        public void TenantId_ShouldGetFromHeader_WhenSetInHttpContext()
        {
            // Arrange
            var tenantId = "header-tenant";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Tenant-Id"] = tenantId;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var result = _accessor.TenantId;

            // Assert
            Assert.Equal(tenantId, result);
            Assert.True(_accessor.HasTenant);
        }
    }
}
