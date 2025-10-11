using System;

namespace QOPIQ.Application.DTOs
{
    public class PrinterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
