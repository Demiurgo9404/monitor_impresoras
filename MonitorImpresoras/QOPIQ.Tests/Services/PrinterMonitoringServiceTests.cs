using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Data;
using QOPIQ.Infrastructure.Services;
using QOPIQ.Infrastructure.Services.SNMP;
using AutoMapper;
using QOPIQ.Application.Mappings;

namespace QOPIQ.Tests.Services
{
    public class PrinterMonitoringServiceTests : IDisposable
    {
        private readonly Mock<ISnmpService> _snmpServiceMock;
        private readonly Mock<IWindowsPrinterService> _windowsPrinterServiceMock;
        private readonly Mock<ILogger<PrinterMonitoringService>> _loggerMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private bool _disposed = false;

        public PrinterMonitoringServiceTests()
        {
            // Configurar DbContext en memoria
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _snmpServiceMock = new Mock<ISnmpService>();
            _windowsPrinterServiceMock = new Mock<IWindowsPrinterService>();
            _loggerMock = new Mock<ILogger<PrinterMonitoringService>>();

            // Configurar AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            _mapper = configuration.CreateMapper();

            // Inicializar datos de prueba
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            // Agregar datos de prueba a la base de datos en memoria
            if (!_dbContext.Printers.Any())
            {
                _dbContext.Printers.AddRange(
                    new Printer
                    {
                        Id = Guid.NewGuid(),
                        Model = "TestPrinter1",
                        IsLocalPrinter = true,
                        IsActive = true,
                        IpAddress = "192.168.1.100"
                    },
                    new Printer
                    {
                        Id = Guid.NewGuid(),
                        Model = "TestPrinter2",
                        IsLocalPrinter = false,
                        IsActive = true,
                        IpAddress = "192.168.1.101"
                    }
                );
                _dbContext.SaveChanges();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task UpdateLocalPrinterStatus_ShouldUpdatePrinterStatus_WhenCalled()
        {
            // Arrange
            var printer = await _dbContext.Printers.FirstOrDefaultAsync(p => p.IsLocalPrinter);
            
            var statusData = new Dictionary<string, object>
            {
                { "IsOnline", true },
                { "IsReady", true },
                { "StatusMessage", "En línea" },
                { "PagesPrinted", 150 },
                { "JobCount", 0 }
            };

            _windowsPrinterServiceMock
                .Setup(x => x.GetPrinterStatusAsync(It.IsAny<string>()))
                .ReturnsAsync(statusData);

            var service = new PrinterMonitoringService(
                _dbContext,
                _windowsPrinterServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            // Act
            await service.UpdateLocalPrinterStatus(printer);

            // Assert
            Assert.Equal("En línea", printer.Status);
            Assert.True(printer.IsOnline);
            Assert.Equal(150, printer.TotalPagesPrinted);
        }

        [Fact]
        public async Task UpdateLocalPrinterStatus_ShouldHandleErrors_WhenPrinterIsNull()
        {
            // Arrange
            var service = new PrinterMonitoringService(
                _dbContext,
                _windowsPrinterServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.UpdateLocalPrinterStatus(null));
        }

        [Fact]
        public async Task CheckAllPrintersStatus_ShouldUpdateAllActivePrinters()
        {
            // Arrange
            var statusData = new Dictionary<string, object>
            {
                { "IsOnline", true },
                { "IsReady", true },
                { "StatusMessage", "En línea" },
                { "PagesPrinted", 200 },
                { "JobCount", 0 }
            };

            _windowsPrinterServiceMock
                .Setup(x => x.GetPrinterStatusAsync(It.IsAny<string>()))
                .ReturnsAsync(statusData);

            var service = new PrinterMonitoringService(
                _dbContext,
                _windowsPrinterServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            // Act
            await service.CheckAllPrintersStatus();

            // Assert
            var updatedPrinters = await _dbContext.Printers.ToListAsync();
            foreach (var printer in updatedPrinters)
            {
                Assert.NotNull(printer.LastChecked);
                if (printer.IsLocalPrinter)
                {
                    Assert.Equal("En línea", printer.Status);
                }
            }
        }

        [Fact]
        public async Task UpdateNetworkPrinterStatus_ShouldUpdateStatus_WhenPrinterIsNetworkPrinter()
        {
            // Arrange
            var printer = await _dbContext.Printers.FirstOrDefaultAsync(p => !p.IsLocalPrinter);
            var snmpResults = new Dictionary<string, object>
            {
                { ".1.3.6.1.2.1.25.3.2.1.5.1", new { Value = 3 } }, // Online
                { ".1.3.6.1.2.1.43.10.2.1.4.1.1", new { Value = 500 } }, // TotalPages
                { ".1.3.6.1.2.1.43.11.1.1.9.1.1", new { Value = 80 } }, // BlackTonerLevel
                { ".1.3.6.1.2.1.43.11.1.1.9.1.2", new { Value = 70 } }, // CyanTonerLevel
                { ".1.3.6.1.2.1.43.11.1.1.9.1.3", new { Value = 75 } }, // MagentaTonerLevel
                { ".1.3.6.1.2.1.43.11.1.1.9.1.4", new { Value = 65 } }  // YellowTonerLevel
            };

            _snmpServiceMock
                .Setup(x => x.GetMultipleAsync(
                    It.IsAny<string>(), 
                    It.IsAny<IEnumerable<string>>(), 
                    It.IsAny<string>(), 
                    It.IsAny<int>()))
                .ReturnsAsync(snmpResults);

            var service = new PrinterMonitoringService(
                _dbContext,
                _windowsPrinterServiceMock.Object,
                _mapper,
                _loggerMock.Object);

            // Act
            await service.UpdateNetworkPrinterStatus(printer);

            // Assert
            Assert.Equal("Online", printer.Status);
            Assert.True(printer.IsOnline);
            Assert.Equal(500, printer.TotalPagesPrinted);
            Assert.Equal(80, printer.BlackTonerLevel);
            Assert.Equal(70, printer.CyanTonerLevel);
            Assert.Equal(75, printer.MagentaTonerLevel);
            Assert.Equal(65, printer.YellowTonerLevel);
        }
    }
}

