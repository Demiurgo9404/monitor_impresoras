using QOPIQ.Domain.Entities;

namespace QOPIQ.Tests.Builders
{
    public class PrinterTestBuilder
    {
        private readonly Printer _printer;

        public PrinterTestBuilder()
        {
            _printer = new Printer
            {
                Id = Guid.NewGuid(),
                Name = "Test Printer",
                IpAddress = "192.168.1.100",
                Model = "Test Model",
                Location = "Test Location",
                Description = "Test Description",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                LastStatusCheck = null,
                IsOnline = false,
                SnmpPort = 161,
                SnmpCommunity = "public",
                PageCount = 0,
                TonerLevel = 100,
                Status = "Unknown"
            };
        }

        public PrinterTestBuilder WithId(Guid id)
        {
            _printer.Id = id;
            return this;
        }

        public PrinterTestBuilder WithName(string name)
        {
            _printer.Name = name;
            return this;
        }

        public PrinterTestBuilder WithIpAddress(string ipAddress)
        {
            _printer.IpAddress = ipAddress;
            return this;
        }

        public PrinterTestBuilder WithSnmpPort(int? port)
        {
            _printer.SnmpPort = port;
            return this;
        }

        public PrinterTestBuilder WithStatus(bool isOnline, DateTime? lastCheck = null)
        {
            _printer.IsOnline = isOnline;
            _printer.LastStatusCheck = lastCheck ?? DateTime.UtcNow;
            return this;
        }

        public PrinterTestBuilder AsInactive()
        {
            _printer.IsActive = false;
            return this;
        }

        public Printer Build()
        {
            return _printer;
        }

        public static implicit operator Printer(PrinterTestBuilder builder)
        {
            return builder.Build();
        }
    }
}
