using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem (aunque no se use directamente aquí)
using TicketSystem.API.DTOs; // Necesario para TicketDto
using TicketSystem.Models;

// Namespace actualizado para la carpeta ViewModels
namespace TicketSystem.ViewModels
{
    public class TicketIndexViewModel
    {
        public IEnumerable<TicketDto> Tickets { get; set; }
        public Dictionary<string, object> DashboardData { get; set; } // Para stats, rol, título

        // Propiedades calculadas para fácil acceso en la vista
        public string UserRole => DashboardData.ContainsKey("UserRole") ? DashboardData["UserRole"] as string : "Other";
        public string ViewTitle => DashboardData.ContainsKey("ViewTitle") ? DashboardData["ViewTitle"] as string : "Tickets";
    }
}
