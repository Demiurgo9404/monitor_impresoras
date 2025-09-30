using System;

namespace MonitorImpresoras.Application.Interfaces
{
    public sealed record PrinterQueryDto(
        Guid Id,
        string Name,
        string IpAddress,
        string? Location
    );
}
