using System;
using System.ComponentModel.DataAnnotations;
using TicketSystem.Models; // Para Enums

namespace TicketSystem.ViewModels
{
    // ViewModel para el formulario de edición de tickets.
    public class TicketEditViewModel
    {
        [Required] // El ID es necesario para identificar el ticket a editar
        public int TicketId { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [MaxLength(200)]
        [Display(Name = "Asunto")]
        public string Subject { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        [Display(Name = "Asignado a")]
        public string? AssignedToUserId { get; set; } // string?

        [Required(ErrorMessage = "La urgencia es obligatoria.")]
        [Display(Name = "Urgencia")]
        public TicketUrgency Urgency { get; set; }

        [Required(ErrorMessage = "La importancia es obligatoria.")]
        [Display(Name = "Importancia")]
        public TicketImportance Importance { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [Display(Name = "Estado")]
        public TicketStatus Status { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Documentación Solución")]
        public string? SolutionDocumentation { get; set; }

        // Propiedades adicionales que podrían ser útiles mostrar en la vista Edit (solo lectura)
        // pero que no se editan directamente aquí (vienen del ticket original).
        // Las añadimos para facilitar el mapeo desde el DTO en el GET.
        [Display(Name = "Creado por")]
        public string CreatedByUserEmail { get; set; }

        [Display(Name = "Fecha Creación")]
        public DateTime CreatedAt { get; set; }

        // Constructor sin parámetros necesario para Model Binding
        public TicketEditViewModel() { }

        // Constructor para facilitar el mapeo desde TicketDto (usado en Edit GET)
        public TicketEditViewModel(API.DTOs.TicketDto dto)
        {
            TicketId = dto.TicketId;
            Subject = dto.Subject;
            Description = dto.Description;
            // Necesitamos los IDs para los selects, no los nombres/strings del DTO
            // CategoryId = dto.CategoryId; // ¡TicketDto no tiene CategoryId! Se necesita cargar por separado o añadir al DTO/Servicio
            // AssignedToUserId = dto.AssignedToUserId; // ¡TicketDto no tiene AssignedToUserId! Se necesita cargar por separado o añadir al DTO/Servicio
            Enum.TryParse(dto.Urgency, out TicketUrgency urgencyEnum); Urgency = urgencyEnum;
            Enum.TryParse(dto.Importance, out TicketImportance importanceEnum); Importance = importanceEnum;
            Enum.TryParse(dto.Status, out TicketStatus statusEnum); Status = statusEnum;
            SolutionDocumentation = dto.SolutionDocumentation;
            CreatedByUserEmail = dto.CreatedByUserEmail;
            CreatedAt = dto.CreatedAt;
            // NOTA: Falta mapear CategoryId y AssignedToUserId aquí.
            // La forma más simple es que GetTicketByIdAsync devuelva también los IDs necesarios,
            // o añadir un método al servicio para obtener solo el Ticket entidad para edición.
            // Por ahora, dejaremos estos sin mapear en este constructor y los asignaremos en la acción Edit GET.
        }
    }
}