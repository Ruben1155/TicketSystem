using System;
using System.ComponentModel.DataAnnotations;
using TicketSystem.Models;

namespace TicketSystem.ViewModels // O TicketSystem.ViewModels si creaste esa carpeta
{
    // ViewModel específico para el formulario de creación de tickets.
    // Contiene solo las propiedades que el usuario debe ingresar.
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [MaxLength(200)]
        [Display(Name = "Asunto")]
        public string Subject { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; } // Descripción es opcional

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; } // El usuario selecciona el ID

        [Display(Name = "Asignado a")]
        public string? AssignedToUserId { get; set; } // El ID del analista es opcional (string?)

        [Required(ErrorMessage = "La urgencia es obligatoria.")]
        [Display(Name = "Urgencia")]
        public TicketUrgency Urgency { get; set; }

        [Required(ErrorMessage = "La importancia es obligatoria.")]
        [Display(Name = "Importancia")]
        public TicketImportance Importance { get; set; }

        // Nota: No incluimos TicketId, CreatedByUserId, CreatedAt, Status, ResolvedAt, etc.
        // porque esos valores se establecen en el servidor o no se definen en la creación.
    }
}
