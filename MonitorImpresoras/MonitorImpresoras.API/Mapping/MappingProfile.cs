using AutoMapper;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Printer mappings
            CreateMap<Printer, PrinterDTO>()
                .ForMember(dest => dest.Consumables, opt => opt.MapFrom(src => src.Consumables));

            CreateMap<CreatePrinterDTO, Printer>();
            CreateMap<UpdatePrinterDTO, Printer>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Printer, PrinterListDTO>()
                .ForMember(dest => dest.TonerLevel, opt => opt.MapFrom(src =>
                    src.Consumables.FirstOrDefault(c => c.Type == "Toner")?.CurrentLevel))
                .ForMember(dest => dest.DrumLevel, opt => opt.MapFrom(src =>
                    src.Consumables.FirstOrDefault(c => c.Type == "Drum")?.CurrentLevel));

            // Consumable mappings
            CreateMap<PrinterConsumable, ConsumableDTO>()
                .ForMember(dest => dest.PrinterName, opt => opt.MapFrom(src => src.Printer.Name));

            CreateMap<PrinterConsumable, PrinterConsumableDTO>();

            // PrintJob mappings
            CreateMap<PrintJob, PrintJobDTO>()
                .ForMember(dest => dest.PrinterName, opt => opt.MapFrom(src => src.Printer.Name))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<CreatePrintJobDTO, PrintJob>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PrintedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.JobStatus, opt => opt.MapFrom(src => "Completed"));

            // Alert mappings
            CreateMap<Alert, AlertDTO>()
                .ForMember(dest => dest.PrinterName, opt => opt.MapFrom(src => src.Printer.Name));

            CreateMap<CreateAlertDTO, Alert>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"));

            CreateMap<UpdateAlertDTO, Alert>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Report mappings
            CreateMap<Report, ReportDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName));

            CreateMap<CreateReportDTO, Report>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            // User mappings (for future use)
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            // Reverse mappings for updates
            CreateMap<PrinterDTO, Printer>();
            CreateMap<ConsumableDTO, PrinterConsumable>();
            CreateMap<PrintJobDTO, PrintJob>();
            CreateMap<AlertDTO, Alert>();
            CreateMap<ReportDTO, Report>();
        }
    }

    // Simple UserDTO for basic user information
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
    }
}
