using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace QOPIQ.Tests
{
    public static class TestHelpers
    {
        public static Mock<DbSet<T>> MockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            return mockSet;
        }

        public static IConfiguration CreateTestConfiguration(Dictionary<string, string> configValues)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            
            return configuration;
        }

        public static ILogger<T> CreateMockLogger<T>()
        {
            var mockLogger = new Mock<ILogger<T>>();
            return mockLogger.Object;
        }
    }
}
