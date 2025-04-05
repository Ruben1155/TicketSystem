using AutoMapper;
using TicketSystem.API.DTOs;
using TicketSystem.Models;
using TicketSystem.ViewModels; // Asegúrate que el namespace de ViewModels sea correcto
using System; // Para Enum.Parse

namespace TicketSystem.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Mapeo de Entidad Ticket -> TicketDto
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.CreatedByUserEmail, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.Email : null))
                .ForMember(dest => dest.AssignedToUserEmail, opt => opt.MapFrom(src => src.AssignedToUser != null ? src.AssignedToUser.Email : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Urgency, opt => opt.MapFrom(src => src.Urgency.ToString()))
                .ForMember(dest => dest.Importance, opt => opt.MapFrom(src => src.Importance.ToString()));
            // Los IDs (CategoryId, CreatedByUserId, AssignedToUserId) se mapean automáticamente por nombre

            // Mapeo de TicketCreateViewModel -> Entidad Ticket
            CreateMap<TicketCreateViewModel, Ticket>()
                .ForMember(dest => dest.TicketId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedToUser, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore());

            // Mapeo de TicketEditViewModel -> Entidad Ticket
            CreateMap<TicketEditViewModel, Ticket>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore()) // Ignorar navegación completa
                .ForMember(dest => dest.AssignedToUser, opt => opt.Ignore()) // Ignorar navegación completa
                .ForMember(dest => dest.Category, opt => opt.Ignore()); // Ignorar navegación completa

            // Mapeo de TicketDto -> TicketEditViewModel
            CreateMap<TicketDto, TicketEditViewModel>()
                .ForMember(dest => dest.Urgency, opt => opt.MapFrom(src => Enum.Parse<TicketUrgency>(src.Urgency, true)))
                .ForMember(dest => dest.Importance, opt => opt.MapFrom(src => Enum.Parse<TicketImportance>(src.Importance, true)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TicketStatus>(src.Status, true)));

            // Mapeo de TicketDto -> TicketDeleteViewModel
            CreateMap<TicketDto, TicketDeleteViewModel>()
                 .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore()); // Se asigna manualmente

            // --- MAPEO FALTANTE AÑADIDO ---
            // Mapeo de TicketDto -> TicketDetailsViewModel
            // Como TicketDetailsViewModel solo tiene una propiedad 'Ticket' de tipo TicketDto,
            // mapeamos la fuente completa (src) a esa propiedad.
            CreateMap<TicketDto, TicketDetailsViewModel>()
                .ForMember(dest => dest.Ticket, opt => opt.MapFrom(src => src));
            // --- FIN MAPEO AÑADIDO ---
        }
    }
}