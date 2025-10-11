using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Infrastructure.Data;
using Xunit;

namespace QOPIQ.Tests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            // Set up the test database
            var serviceCollection = new ServiceCollection();
            
            // Use in-memory database for testing
            serviceCollection.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Register services
            serviceCollection.AddScoped<IPrinterRepository, PrinterRepository>();
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            // Ensure the database is created
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task CommitTransaction_ShouldSaveChanges()
        {
            // Arrange
            var printer = new Printer
            {
                Id = Guid.NewGuid(),
                Name = "Test Printer",
                IpAddress = "192.168.1.100",
                IsActive = true,
                Created = DateTime.UtcNow
            };

            // Act
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.Printers.AddAsync(printer);
            await _unitOfWork.CommitTransactionAsync();

            // Assert
            var savedPrinter = await _unitOfWork.Printers.GetByIdAsync(printer.Id);
            Assert.NotNull(savedPrinter);
            Assert.Equal(printer.Name, savedPrinter.Name);
        }

        [Fact]
        public async Task RollbackTransaction_ShouldNotSaveChanges()
        {
            // Arrange
            var printer = new Printer
            {
                Id = Guid.NewGuid(),
                Name = "Test Printer Rollback",
                IpAddress = "192.168.1.101",
                IsActive = true,
                Created = DateTime.UtcNow
            };

            try
            {
                // Act
                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.Printers.AddAsync(printer);
                
                // Simulate an error
                throw new Exception("Test exception");
                
                // This line won't be reached
                // await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                // Explicit rollback on error
                await _unitOfWork.RollbackTransactionAsync();
            }

            // Assert
            var savedPrinter = await _unitOfWork.Printers.GetByIdAsync(printer.Id);
            Assert.Null(savedPrinter);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _serviceProvider.Dispose();
        }
    }
}
