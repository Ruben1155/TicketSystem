using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem (aunque no se use directamente aquí)
using TicketSystem.API.DTOs; // Necesario para TicketDto
using TicketSystem.Models;

namespace TicketSystem.ViewModels
{
    public class TicketDetailsViewModel
    {
        public TicketDto Ticket { get; set; }
        // Puedes añadir más propiedades si la vista Details necesita otros datos
        // Por ejemplo: public bool CanEdit { get; set; }
        // public bool CanDelete { get; set; }
    }
}
