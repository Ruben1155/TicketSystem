using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem (aunque no se use directamente aquí)
using TicketSystem.API.DTOs; // Necesario para TicketDto
using TicketSystem.Models;


namespace TicketSystem.ViewModels
{
    public class TicketDeleteViewModel
    {
        // Necesitamos el ID para el formulario POST
        [Required] // Asegura que el ID se envíe de vuelta
        public int TicketId { get; set; }

        // Propiedades del DTO para mostrar en la confirmación
        public string Subject { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }
        public string CreatedByUserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AssignedToUserEmail { get; set; }

        // Para mostrar errores si la eliminación falla
        public string ErrorMessage { get; set; }

        // Constructor opcional para facilitar el mapeo desde el DTO en el controlador
        public TicketDeleteViewModel() { } // Constructor sin parámetros necesario para model binding

        public TicketDeleteViewModel(TicketDto ticketDto)
        {
            this.TicketId = ticketDto.TicketId;
            this.Subject = ticketDto.Subject;
            this.CategoryName = ticketDto.CategoryName;
            this.Status = ticketDto.Status;
            this.CreatedByUserEmail = ticketDto.CreatedByUserEmail;
            this.CreatedAt = ticketDto.CreatedAt;
            this.AssignedToUserEmail = ticketDto.AssignedToUserEmail;
        }
    }
}
