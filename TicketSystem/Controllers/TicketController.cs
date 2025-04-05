using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore; // Ya no se necesita aquí
using System.Threading.Tasks;
using TicketSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.Extensions.Logging;
using System;
using TicketSystem.Services;
using TicketSystem.API.DTOs; // Para TicketDto
using TicketSystem.ViewModels; // Para los ViewModels MVC
using AutoMapper; // <-- AÑADIDO: Using para AutoMapper

namespace TicketSystem.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        // Inyecta los servicios necesarios y AutoMapper
        private readonly ITicketService _ticketService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TicketController> _logger;
        private readonly IMapper _mapper; // <-- AÑADIDO

        // Constructor actualizado para inyectar IMapper y quitar ApplicationDbContext
        public TicketController(
            ITicketService ticketService,
            UserManager<User> userManager,
            ILogger<TicketController> logger,
            IMapper mapper) // <-- AÑADIDO
        {
            _ticketService = ticketService;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper; // <-- AÑADIDO
            // Ya no necesitamos _context aquí
        }

        // GET: Ticket
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }
            var dashboardData = await _ticketService.GetDashboardDataAsync(currentUser);
            var ticketsDto = await _ticketService.GetTicketsForUserAsync(currentUser);
            var viewModel = new TicketIndexViewModel { Tickets = ticketsDto, DashboardData = dashboardData };
            return View(viewModel);
        }

        // GET: Ticket/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) { return NotFound(); }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }
            var ticketDto = await _ticketService.GetTicketByIdAsync(id.Value, currentUser);
            if (ticketDto == null) { TempData["ErrorMessage"] = "Ticket no encontrado o no tiene permiso."; return RedirectToAction(nameof(Index)); }
            // Mapear DTO a ViewModel usando AutoMapper
            var viewModel = _mapper.Map<TicketDetailsViewModel>(ticketDto);
            return View(viewModel);
        }

        // GET: Ticket/DailyResolvedReport
        public async Task<IActionResult> DailyResolvedReport()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }
            var resolvedTodayDtos = await _ticketService.GetDailyResolvedTicketsAsync(currentUser);
            var viewModel = new DailyResolvedReportViewModel { Tickets = resolvedTodayDtos, ReportDate = DateTime.UtcNow.ToString("D") };
            return View(viewModel);
        }


        // GET: Ticket/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Role != UserRole.Support) { return Forbid(); }
            await PopulateDropdownsFromServiceAsync();
            return View(new TicketCreateViewModel());
        }

        // POST: Ticket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCreateViewModel viewModel) // Recibe ViewModel
        {
            var creatorUser = await _userManager.GetUserAsync(User);
            if (creatorUser == null) { return Challenge(); }
            if (creatorUser.Role != UserRole.Support) { return Forbid(); }

            if (ModelState.IsValid) // Valida ViewModel
            {
                // Mapea ViewModel a entidad Ticket usando AutoMapper
                var ticket = _mapper.Map<Ticket>(viewModel); // <-- USA AUTOMAPPER

                var (success, errorMessage, createdTicketDto) = await _ticketService.CreateTicketAsync(ticket, creatorUser);
                if (success) { TempData["SuccessMessage"] = "Ticket creado exitosamente!"; return RedirectToAction(nameof(Index)); }
                else { ModelState.AddModelError(string.Empty, errorMessage ?? "Ocurrió un error."); }
            }
            else { /* Log errores si es necesario */ }
            await PopulateDropdownsFromServiceAsync(viewModel.CategoryId, viewModel.AssignedToUserId);
            return View(viewModel); // Devuelve ViewModel
        }

        // GET: Ticket/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) { return NotFound(); }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }

            // Obtener DTO desde el servicio (valida permiso base para ver)
            var ticketDto = await _ticketService.GetTicketByIdAsync(id.Value, currentUser);
            if (ticketDto == null) { TempData["ErrorMessage"] = "Ticket no encontrado o no tiene permiso."; return RedirectToAction(nameof(Index)); }

            // Mapear DTO a EditViewModel usando AutoMapper
            // El perfil de mapeo se encarga de convertir los enums string a enums
            // y copiar los IDs (CategoryId, AssignedToUserId) que añadimos al DTO.
            var viewModel = _mapper.Map<TicketEditViewModel>(ticketDto); // <-- USA AUTOMAPPER

            // Verificar permiso específico para editar (Analista asignado, Soporte creador, Admin)
            // Usamos los IDs que ahora están en el DTO/ViewModel
            bool isAdmin = currentUser.Role == UserRole.Admin;
            // Asegurarse que AssignedToUserId (string?) se compara correctamente
            bool isAssignee = !string.IsNullOrEmpty(viewModel.AssignedToUserId) && currentUser.Id == viewModel.AssignedToUserId;
            bool isCreator = currentUser.Id == ticketDto.CreatedByUserId; // Usa ID del DTO original
            bool canEdit = (isAssignee && currentUser.Role == UserRole.Analyst) ||
                           (isCreator && currentUser.Role == UserRole.Support) ||
                           isAdmin;
            if (!canEdit) { _logger.LogWarning($"Usuario {currentUser.Email} (Rol: {currentUser.Role}) intentó editar ticket {id} sin permiso."); TempData["ErrorMessage"] = "No tiene permiso para editar este ticket."; return RedirectToAction(nameof(Details), new { id = id }); }

            // Elimina la consulta temporal a _context que teníamos antes

            await PopulateDropdownsFromServiceAsync(viewModel.CategoryId, viewModel.AssignedToUserId); // Usa IDs del ViewModel
            return View(viewModel); // <-- Pasa TicketEditViewModel
        }

        // POST: Ticket/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TicketEditViewModel viewModel) // <-- Recibe TicketEditViewModel
        {
            if (id != viewModel.TicketId) { return NotFound(); }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }

            // Ya no necesitamos remover ModelState si el ViewModel está bien definido y validado

            if (ModelState.IsValid) // Valida el ViewModel
            {
                // Llama al servicio pasando el ViewModel directamente
                // (El servicio fue actualizado para aceptar TicketEditViewModel)
                var (success, errorMessage) = await _ticketService.UpdateTicketAsync(viewModel, currentUser); // <-- Pasa ViewModel

                if (success)
                {
                    TempData["SuccessMessage"] = "Ticket actualizado exitosamente!";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage ?? "Ocurrió un error.");
                    if (errorMessage == "Ticket no encontrado.") return NotFound();
                    if (errorMessage == "No tiene permiso para editar este ticket.") return Forbid();
                }
            }
            else { /* Log errores */ }

            // Repoblar si falla
            await PopulateDropdownsFromServiceAsync(viewModel.CategoryId, viewModel.AssignedToUserId); // <-- Usa IDs del ViewModel
            return View(viewModel); // <-- Devuelve el ViewModel con errores
        }


        // GET: Ticket/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null) { return NotFound(); }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }

            // Obtener DTO desde el servicio (valida permiso para ver)
            var ticketDto = await _ticketService.GetTicketByIdAsync(id.Value, currentUser);
            if (ticketDto == null) { TempData["ErrorMessage"] = "Ticket no encontrado o no tiene permiso."; return RedirectToAction(nameof(Index)); }

            // Mapear DTO a DeleteViewModel usando AutoMapper
            var viewModel = _mapper.Map<TicketDeleteViewModel>(ticketDto); // <-- USA AUTOMAPPER
            viewModel.ErrorMessage = saveChangesError.GetValueOrDefault() ? "Error al eliminar..." : null;

            // Re-validar permiso específico para borrar (Creador o Admin)
            // Usamos el CreatedByUserId que ahora está en el DTO (y por ende en el ViewModel mapeado)
            bool isAdmin = currentUser.Role == UserRole.Admin;
            if (currentUser.Id != ticketDto.CreatedByUserId && !isAdmin) // <-- Usa ID del DTO
            {
                TempData["ErrorMessage"] = "No tiene permiso para eliminar este ticket.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Elimina la consulta temporal a _context que teníamos antes

            return View(viewModel); // Pasar ViewModel
        }

        // POST: Ticket/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) // Recibe ID
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) { return Challenge(); }
            // Llama al servicio que valida permiso y elimina
            var (success, errorMessage) = await _ticketService.DeleteTicketAsync(id, currentUser);
            if (success) { TempData["SuccessMessage"] = "Ticket eliminado exitosamente!"; return RedirectToAction(nameof(Index)); }
            else { _logger.LogError($"Error al eliminar ticket {id} por usuario {currentUser.Email}: {errorMessage}"); TempData["ErrorMessage"] = errorMessage ?? "Error al eliminar el ticket."; return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true }); }
        }

        // Helper para poblar Dropdowns (sin cambios)
        private async Task PopulateDropdownsFromServiceAsync(object? selectedCategory = null, object? selectedAnalyst = null)
        {
            var (categories, analysts) = await _ticketService.GetDropdownDataAsync();
            ViewBag.CategoryId = new SelectList(categories, "Value", "Text", selectedCategory);
            ViewBag.AssignedToUserId = new SelectList(analysts, "Value", "Text", selectedAnalyst as string);
        }

        // Ya no necesitamos _context aquí
    }
}
