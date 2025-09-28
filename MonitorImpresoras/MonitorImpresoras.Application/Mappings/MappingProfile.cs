using AutoMapper;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeo de Printer a PrinterDto y viceversa
            CreateMap<Printer, PrinterDto>();
            CreateMap<PrinterDto, Printer>();
            
            // Mapeo para la creación de impresoras
            CreateMap<CreatePrinterDto, Printer>();
            
            // Mapeo para la actualización de impresoras
            CreateMap<UpdatePrinterDto, Printer>();
            
            // Mapeo para el estado de la impresora
            CreateMap<Printer, PrinterStatusDto>();
        }
    }
}
