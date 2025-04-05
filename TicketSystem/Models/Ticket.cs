// Models/Ticket.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystem.Models // Asegúrate que el namespace coincida
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [MaxLength(200)]
        [Display(Name = "Asunto")]
        public string Subject { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }

        // --- Claves Foráneas y Navegación (Actualizadas) ---

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // Clave Foránea para el usuario creador (Soporte) - AHORA ES STRING
        [Required]
        public string CreatedByUserId { get; set; } // Cambiado de int a string

        // Propiedad de navegación al usuario creador (tipo User actualizado)
        [ForeignKey("CreatedByUserId")]
        [Display(Name = "Creado por")]
        public virtual User? CreatedByUser { get; set; } // Tipo User (nuestro IdentityUser extendido)

        // Clave Foránea para el usuario asignado (Analista) - AHORA ES STRING? (nullable)
        [Display(Name = "Asignado a")]
        public string? AssignedToUserId { get; set; } // Cambiado de int? a string?

        // Propiedad de navegación al usuario asignado (tipo User actualizado)
        [ForeignKey("AssignedToUserId")]
        public virtual User? AssignedToUser { get; set; } // Tipo User

        // --- Otras Propiedades (Sin cambios) ---

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

        [Display(Name = "Fecha Creación")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Fecha Resolución")]
        public DateTime? ResolvedAt { get; set; }

        [Display(Name = "Última Actualización")]
        public DateTime? LastUpdatedAt { get; set; }
    }
}
