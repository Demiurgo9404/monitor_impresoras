using QOPIQ.Application.Interfaces;
using QOPIQ.Infrastructure.Services;
using Xunit;

namespace QOPIQ.Tests
{
    public class TenantAccessorTests
    {
        [Fact]
        public void SetTenant_ShouldSetTenantIdAndInfo()
        {
            // Arrange
            var accessor = new TenantAccessor();
            var tenantId = "test-tenant";
            var tenantInfo = new TenantInfo
            {
                TenantId = tenantId,
                Name = "Test Tenant",
                CompanyName = "Test Company",
                IsActive = true
            };

            // Act
            accessor.SetTenant(tenantId, tenantInfo);

            // Assert
            Assert.Equal(tenantId, accessor.TenantId);
            Assert.Equal(tenantInfo, accessor.TenantInfo);
            Assert.True(accessor.HasTenant);
        }

        [Fact]
        public void HasTenant_ShouldReturnFalse_WhenNoTenantSet()
        {
            // Arrange
            var accessor = new TenantAccessor();

            // Act & Assert
            Assert.False(accessor.HasTenant);
            Assert.Null(accessor.TenantId);
            Assert.Null(accessor.TenantInfo);
        }

        [Fact]
        public void SetTenant_WithNullInfo_ShouldSetOnlyTenantId()
        {
            // Arrange
            var accessor = new TenantAccessor();
            var tenantId = "test-tenant";

            // Act
            accessor.SetTenant(tenantId);

            // Assert
            Assert.Equal(tenantId, accessor.TenantId);
            Assert.Null(accessor.TenantInfo);
            Assert.True(accessor.HasTenant);
        }

        [Fact]
        public async Task TenantAccessor_ShouldBeThreadSafe()
        {
            // Arrange
            var accessor1 = new TenantAccessor();
            var accessor2 = new TenantAccessor();

            // Act
            var task1 = Task.Run(() =>
            {
                accessor1.SetTenant("tenant1");
                Thread.Sleep(100);
                return accessor1.TenantId;
            });

            var task2 = Task.Run(() =>
            {
                accessor2.SetTenant("tenant2");
                Thread.Sleep(100);
                return accessor2.TenantId;
            });

            var results = await Task.WhenAll(task1, task2);

            // Assert
            Assert.Equal("tenant1", results[0]);
            Assert.Equal("tenant2", results[1]);
        }
    }
}

