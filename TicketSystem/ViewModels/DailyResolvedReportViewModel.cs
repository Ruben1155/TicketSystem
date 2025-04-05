using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem (aunque no se use directamente aquí)
using TicketSystem.API.DTOs; // Necesario para TicketDto
using TicketSystem.Models;

namespace TicketSystem.ViewModels
{
    public class DailyResolvedReportViewModel
    {
        public IEnumerable<TicketDto> Tickets { get; set; }
        public string ReportDate { get; set; }
    }
}
