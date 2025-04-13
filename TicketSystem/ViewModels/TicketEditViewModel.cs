using System;
using System.ComponentModel.DataAnnotations;
using TicketSystem.Models; // Para Enums

namespace TicketSystem.ViewModels
{
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

        // Constructor que recibe un objeto Ticket y mapea sus propiedades.
        public TicketEditViewModel(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            Subject = ticket.Subject;
            Description = ticket.Description;
            CategoryId = ticket.CategoryId;
            AssignedToUserId = ticket.AssignedToUserId;
            Urgency = ticket.Urgency;
            Importance = ticket.Importance;
            Status = ticket.Status;
            SolutionDocumentation = ticket.SolutionDocumentation;
            CreatedByUserEmail = ticket.CreatedByUser?.Email; // Accede a la propiedad de navegación CreatedByUser
            CreatedAt = ticket.CreatedAt;
        }
    }
}