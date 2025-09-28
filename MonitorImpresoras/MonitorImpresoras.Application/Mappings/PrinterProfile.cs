using AutoMapper;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Application.DTOs.Printers;

namespace MonitorImpresoras.Application.Mappings
{
    /// <summary>
    /// Perfil de AutoMapper para mapeo de entidades Printer
    /// </summary>
    public class PrinterProfile : Profile
    {
        public PrinterProfile()
        {
            // Mapeo de Printer a PrinterDto (para respuestas)
            CreateMap<Printer, PrinterDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // Mapeo de CreatePrinterDto a Printer (para creación)
            CreateMap<CreatePrinterDto, Printer>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastSeen, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsOnline, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsLocalPrinter, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.LastMaintenance, opt => opt.MapFrom(src => (DateTime?)null))
                .ForMember(dest => dest.MaintenanceIntervalDays, opt => opt.MapFrom(src => 90))
                .ForMember(dest => dest.LastChecked, opt => opt.MapFrom(src => (DateTime?)null))
                .ForMember(dest => dest.PageCount, opt => opt.MapFrom(src => (int?)null))
                .ForMember(dest => dest.LastError, opt => opt.MapFrom(src => string.Empty));

            // Mapeo de UpdatePrinterDto a Printer (para actualizaciones)
            CreateMap<UpdatePrinterDto, Printer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
