using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketSystem.Models;
using TicketSystem.Services;
using TicketSystem.API.DTOs;
using TicketSystem.ViewModels; // <-- AÑADIDO: Using para ViewModels (TicketEditViewModel)
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TicketSystem.API
{
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TicketsController> _logger;
        // IMapper no es necesario aquí si el servicio maneja DTOs y el controller recibe ViewModels/DTOs específicos

        public TicketsController(
            ITicketService ticketService,
            UserManager<User> userManager,
            ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<User> GetCurrentUserAsync() { /* ... (Método helper temporal) ... */ return await _userManager.GetUserAsync(User); }

        // GET: api/Tickets
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TicketDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTickets()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) { return StatusCode(StatusCodes.Status501NotImplemented, "Autenticación de API no implementada."); }
            try { var ticketsDto = await _ticketService.GetTicketsForUserAsync(currentUser); return Ok(ticketsDto); }
            catch (Exception ex) { _logger.LogError(ex, "Error GetTickets API: {UserId}", currentUser?.Id); return StatusCode(StatusCodes.Status500InternalServerError, "Error interno."); }
        }

        // GET: api/Tickets/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TicketDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) { return StatusCode(StatusCodes.Status501NotImplemented, "Autenticación de API no implementada."); }
            try { var ticketDto = await _ticketService.GetTicketByIdAsync(id, currentUser); if (ticketDto == null) { return NotFound(); } return Ok(ticketDto); }
            catch (Exception ex) { _logger.LogError(ex, "Error GetTicket({TId}) API: {UserId}", id, currentUser?.Id); return StatusCode(StatusCodes.Status500InternalServerError, "Error interno."); }
        }

        // POST: api/Tickets
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TicketDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult<TicketDto>> CreateTicket(TicketCreateViewModel viewModel) // Usa CreateViewModel
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) { return StatusCode(StatusCodes.Status501NotImplemented, "Autenticación de API no implementada."); }
            if (currentUser.Role != UserRole.Support) { return Forbid(); }
            // [ApiController] valida viewModel
            try
            {
                // El servicio necesita Ticket, no ViewModel. Mapeo manual o AutoMapper aquí.
                // Usaremos mapeo manual aquí por simplicidad para la API POST
                var ticket = new Ticket { Subject = viewModel.Subject, Description = viewModel.Description, CategoryId = viewModel.CategoryId, AssignedToUserId = viewModel.AssignedToUserId, Urgency = viewModel.Urgency, Importance = viewModel.Importance };
                var (success, errorMessage, createdTicketDto) = await _ticketService.CreateTicketAsync(ticket, currentUser); // Servicio devuelve DTO
                if (success) { return CreatedAtAction(nameof(GetTicket), new { id = createdTicketDto.TicketId }, createdTicketDto); }
                else { return BadRequest(new { message = errorMessage ?? "Error al crear." }); }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error CreateTicket API: {UserId}", currentUser?.Id); return StatusCode(StatusCodes.Status500InternalServerError, "Error interno."); }
        }


        // PUT: api/Tickets/5
        // CAMBIO: Acepta TicketEditViewModel
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Añadido para ID mismatch
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<IActionResult> UpdateTicket(int id, TicketEditViewModel viewModel) // <-- CAMBIO: Acepta ViewModel
        {
            // Validar que el ID en la ruta coincida con el ID en el cuerpo
            if (id != viewModel.TicketId)
            {
                return BadRequest("El ID del ticket en la ruta no coincide con el ID en el cuerpo.");
            }

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) { return StatusCode(StatusCodes.Status501NotImplemented, "Autenticación de API no implementada."); }

            // [ApiController] valida el viewModel si tiene atributos [Required], etc.

            try
            {
                // Llamar al servicio pasando el ViewModel directamente
                var (success, errorMessage) = await _ticketService.UpdateTicketAsync(viewModel, currentUser); // <-- CAMBIO: Pasa ViewModel

                if (success)
                {
                    return NoContent(); // 204
                }
                else
                {
                    // Analizar el error devuelto por el servicio
                    if (errorMessage == "Ticket no encontrado.") return NotFound();
                    if (errorMessage == "No tiene permiso para editar este ticket.") return Forbid();
                    return BadRequest(new { message = errorMessage ?? "Error al actualizar." });
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error UpdateTicket({TId}) API: {UserId}", id, currentUser?.Id); return StatusCode(StatusCodes.Status500InternalServerError, "Error interno."); }
        }

        // DELETE: api/Tickets/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) { return StatusCode(StatusCodes.Status501NotImplemented, "Autenticación de API no implementada."); }

            try
            {
                var (success, errorMessage) = await _ticketService.DeleteTicketAsync(id, currentUser);
                if (success) { return NoContent(); } // 204
                else { if (errorMessage == "Ticket no encontrado.") return NotFound(); if (errorMessage == "No tiene permiso para eliminar este ticket.") return Forbid(); return BadRequest(new { message = errorMessage ?? "Error al eliminar." }); }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error DeleteTicket({TId}) API: {UserId}", id, currentUser?.Id); return StatusCode(StatusCodes.Status500InternalServerError, "Error interno."); }
        }
    }
}