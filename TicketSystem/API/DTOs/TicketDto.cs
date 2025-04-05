using System;
using TicketSystem.Models; // Para las enumeraciones

namespace TicketSystem.API.DTOs
{
    public class TicketDto
    {
        public int TicketId { get; set; }
        public string Subject { get; set; }
        public string? Description { get; set; }

        // --- Datos Relacionados Aplanados ---
        public string CategoryName { get; set; }
        public string CreatedByUserEmail { get; set; }
        public string? AssignedToUserEmail { get; set; }

        // --- IDs (Añadidos para facilitar mapeo a ViewModels) ---
        public int CategoryId { get; set; }
        public string CreatedByUserId { get; set; } // string (GUID)
        public string? AssignedToUserId { get; set; } // string? (GUID)

        // --- Otros Datos ---
        public string Status { get; set; }
        public string Urgency { get; set; }
        public string Importance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string? SolutionDocumentation { get; set; }
    }
}