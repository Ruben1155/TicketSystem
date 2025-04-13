using AutoMapper; // <-- AÑADIDO: Using para AutoMapper
using AutoMapper.QueryableExtensions; // <-- AÑADIDO: Using para .ProjectTo()
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketSystem.Data;
using TicketSystem.Models;
using TicketSystem.API.DTOs; // Using para DTOs
using TicketSystem.Services; // Using para la interfaz
using TicketSystem.ViewModels; // Using para ViewModels (necesario para UpdateTicketAsync)

namespace TicketSystem.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<TicketService> _logger;
        private readonly IMapper _mapper; // <-- AÑADIDO: Inyección de AutoMapper

        public TicketService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<TicketService> logger,
            IMapper mapper) // <-- AÑADIDO: Parámetro IMapper
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper; // <-- AÑADIDO: Asignación de IMapper
        }

        // CAMBIO: Usa ProjectTo de AutoMapper
        public async Task<IEnumerable<TicketDto>> GetTicketsForUserAsync(User currentUser)
        {
            _logger.LogInformation($"Obteniendo tickets para usuario {currentUser.Email} (Rol: {currentUser.Role})");
            IQueryable<Ticket> query = _context.Tickets; // ProjectTo necesita IQueryable<Ticket>

            // Aplicar filtro por rol
            if (currentUser.Role == UserRole.Support) { query = query.Where(t => t.CreatedByUserId == currentUser.Id); }
            else if (currentUser.Role == UserRole.Analyst) { query = query.Where(t => t.AssignedToUserId == currentUser.Id); }
            else if (currentUser.Role != UserRole.Admin) { query = query.Where(t => false); } // Si no es Admin, Support o Analyst, no muestra nada

            // Mapeo usando AutoMapper.ProjectTo
            // Nota: ProjectTo es inteligente y solo incluirá las columnas necesarias
            // para el DTO, optimizando la consulta SQL. No necesita los .Include() explícitos aquí
            // si el perfil de mapeo está bien configurado para resolver nombres/emails.
            // PERO: Si el mapeo en el Profile USA las propiedades de navegación (ej. src.Category.Name),
            // SÍ necesitas los Include() ANTES del ProjectTo. Vamos a mantenerlos por seguridad.
            query = query.Include(t => t.Category)
                         .Include(t => t.CreatedByUser)
                         .Include(t => t.AssignedToUser);

            return await query.OrderByDescending(t => t.CreatedAt)
                              .ProjectTo<TicketDto>(_mapper.ConfigurationProvider) // <-- CAMBIO: Usa ProjectTo
                              .AsNoTracking()
                              .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetDashboardDataAsync(User currentUser)
        {
            // La lógica de estadísticas no necesita mapeo, sigue igual
            var dashboardData = new Dictionary<string, object>();
            var today = DateTime.UtcNow.Date;
            var baseTicketsQuery = _context.Tickets;
            if (currentUser.Role == UserRole.Admin) { /*...*/ }
            else if (currentUser.Role == UserRole.Support) { /*...*/ }
            else if (currentUser.Role == UserRole.Analyst) { /*...*/ }
            else { /*...*/ }
            return dashboardData;
        }

        // CAMBIO: Usa Map de AutoMapper
        public async Task<TicketDto> GetTicketByIdAsync(int id, User currentUser)
        {
            _logger.LogInformation($"Usuario {currentUser.Email} buscando ticket ID {id}");
            // Para mapear un solo objeto, necesitamos incluir las navegaciones que usa el mapeo
            var ticket = await _context.Tickets
               .Include(t => t.Category)
               .Include(t => t.CreatedByUser)
               .Include(t => t.AssignedToUser)
               .AsNoTracking() // Para lectura
               .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null) { return null; }

            bool isAdmin = currentUser.Role == UserRole.Admin;
            if (currentUser.Id != ticket.CreatedByUserId && currentUser.Id != ticket.AssignedToUserId && !isAdmin)
            {
                _logger.LogWarning($"Usuario {currentUser.Email} intentó acceder al ticket {id} sin permiso.");
                return null;
            }

            // Mapear a DTO usando AutoMapper
            return _mapper.Map<TicketDto>(ticket); // <-- CAMBIO
        }

        public async Task<Ticket> GetTicketEntityByIdAsync(int id, User currentUser)
        {
            _logger.LogInformation($"Usuario {currentUser.Email} buscando entidad de ticket ID {id}");

            // Obtener la entidad Ticket con las relaciones necesarias
            var ticket = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(m => m.TicketId == id);

            if (ticket == null)
            {
                _logger.LogWarning($"Ticket con ID {id} no encontrado.");
                return null;
            }

            bool isAdmin = currentUser.Role == UserRole.Admin;
            if (currentUser.Id != ticket.CreatedByUserId && currentUser.Id != ticket.AssignedToUserId && !isAdmin)
            {
                _logger.LogWarning($"Usuario {currentUser.Email} intentó acceder al ticket {id} sin permiso.");
                return null;
            }
            return ticket;
        }
        public async Task<(IEnumerable<SelectListItem> Categories, IEnumerable<SelectListItem> Analysts)> GetDropdownDataAsync()
        {
            // Sin cambios aquí
            var categories = await _context.Categories.OrderBy(c => c.Name).Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name }).AsNoTracking().ToListAsync();
            var analysts = await _userManager.Users.Where(u => u.Role == UserRole.Analyst).OrderBy(u => u.Email).Select(u => new SelectListItem { Value = u.Id, Text = u.Email }).AsNoTracking().ToListAsync();
            return (categories, analysts);
        }

        // CAMBIO: Usa Map de AutoMapper para devolver DTO
        public async Task<(bool Success, string ErrorMessage, TicketDto CreatedTicket)> CreateTicketAsync(Ticket ticket, User creatorUser)
        {
            if (creatorUser.Role != UserRole.Support) { return (false, "Usuario no autorizado.", null); }

            ticket.CreatedByUserId = creatorUser.Id;
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = TicketStatus.Creado;
            ticket.TicketId = 0;

            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Ticket {ticket.TicketId} creado por {creatorUser.Email}");

                // Mapear la entidad guardada (que ahora tiene ID) a DTO
                // Necesitamos cargar navegaciones si no están ya y el mapeo las usa
                await _context.Entry(ticket).Reference(t => t.Category).LoadAsync();
                // CreatedByUser es el objeto creatorUser pasado, lo asignamos al ticket antes del mapeo si es necesario
                ticket.CreatedByUser = creatorUser; // Asegurar que esté para el mapeo
                if (!string.IsNullOrEmpty(ticket.AssignedToUserId)) await _context.Entry(ticket).Reference(t => t.AssignedToUser).LoadAsync();

                var createdDto = _mapper.Map<TicketDto>(ticket); // <-- CAMBIO

                return (true, null, createdDto);
            }
            catch (Exception ex) { _logger.LogError(ex, $"Error al crear ticket por {creatorUser.Email}"); return (false, "Error al guardar...", null); }
        }

        // CAMBIO: Firma acepta TicketEditViewModel, Usa Map de AutoMapper para actualizar la entidad
        public async Task<(bool Success, string ErrorMessage)> UpdateTicketAsync(TicketEditViewModel viewModel, User currentUser)
        {
            var ticketToUpdate = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == viewModel.TicketId);
            if (ticketToUpdate == null) { return (false, "Ticket no encontrado."); }

            // Valida permiso usando la entidad original
            bool isAdmin = currentUser.Role == UserRole.Admin;
            bool isAssignee = currentUser.Id == ticketToUpdate.AssignedToUserId;
            bool isCreator = currentUser.Id == ticketToUpdate.CreatedByUserId;
            bool canEdit = (isAssignee && currentUser.Role == UserRole.Analyst) || (isCreator && currentUser.Role == UserRole.Support) || isAdmin;
            if (!canEdit) { return (false, "No tiene permiso para editar este ticket."); }

            try
            {
                // Mapea desde el ViewModel recibido a la entidad existente rastreada
                _mapper.Map(viewModel, ticketToUpdate); // <-- CAMBIO: Usa AutoMapper para actualizar

                // Asigna manualmente campos que no deben venir del mapeo o necesitan lógica
                ticketToUpdate.LastUpdatedAt = DateTime.UtcNow;
                // Si el ticket se marca como Resuelto y ResolvedAt no tiene valor,
                // asignamos la fecha y hora actual.
                if (ticketToUpdate.Status == TicketStatus.Resuelto && !ticketToUpdate.ResolvedAt.HasValue) 
                { ticketToUpdate.ResolvedAt = DateTime.UtcNow; }
                // Si el ticket no está Resuelto, nos aseguramos que ResolvedAt sea null.
                else if (ticketToUpdate.Status != TicketStatus.Resuelto) 
                { ticketToUpdate.ResolvedAt = null; }
                // CreatedByUserId es ignorado por el perfil de mapeo, no se cambiará

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Ticket {ticketToUpdate.TicketId} actualizado por {currentUser.Email}");
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex) { _logger.LogError(ex, $"Error de concurrencia editando ticket {viewModel.TicketId}."); return (false, "Error de concurrencia..."); }
            catch (Exception ex) { _logger.LogError(ex, $"Error guardando cambios en ticket {viewModel.TicketId}."); return (false, "Ocurrió un error inesperado..."); }
        }

        public async Task<(bool Success, string ErrorMessage)> DeleteTicketAsync(int id, User currentUser)
        {
            // Sin cambios de mapeo aquí
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) { return (false, "Ticket no encontrado."); }
            bool isAdmin = currentUser.Role == UserRole.Admin;
            if (currentUser.Id != ticket.CreatedByUserId && !isAdmin) { return (false, "No tiene permiso..."); }
            try { _context.Tickets.Remove(ticket); await _context.SaveChangesAsync(); /*...*/ return (true, null); }
            catch (DbUpdateException ex) { /*...*/ return (false, "Error al eliminar..."); }
            catch (Exception ex) { /*...*/ return (false, "Ocurrió un error inesperado..."); }
        }

        // CAMBIO: Usa ProjectTo de AutoMapper
        public async Task<IEnumerable<TicketDto>> GetDailyResolvedTicketsAsync(User currentUser)
        {
            if (currentUser.Role != UserRole.Analyst) { return new List<TicketDto>(); }
            var today = DateTime.UtcNow.Date;
            // ProjectTo se encarga de seleccionar solo las columnas necesarias
            // Mantener Include si el Profile los necesita para mapear CategoryName/CreatedUserEmail
            IQueryable<Ticket> query = _context.Tickets
               .Include(t => t.Category)
               .Include(t => t.CreatedByUser);

            return await query
                .Where(t => t.AssignedToUserId == currentUser.Id &&
                            t.ResolvedAt.HasValue &&
                            t.ResolvedAt.Value.Date == today)
                .OrderByDescending(t => t.ResolvedAt)
                .ProjectTo<TicketDto>(_mapper.ConfigurationProvider) // <-- CAMBIO
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> TicketExistsAsync(int id) { return await _context.Tickets.AnyAsync(e => e.TicketId == id); }
    }
}
