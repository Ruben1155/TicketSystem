using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering; // Para SelectListItem
using TicketSystem.Models;
using TicketSystem.API.DTOs; // Using para DTOs
using TicketSystem.ViewModels; // <-- Using para ViewModels

namespace TicketSystem.Services
{
    /// <summary>
    /// Define el contrato para el servicio que maneja la lógica de negocio de los tickets.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Obtiene los tickets (como DTOs) visibles para un usuario específico según su rol.
        /// </summary>
        /// <param name="currentUser">El usuario actualmente autenticado.</param>
        /// <returns>Una colección de TicketDto.</returns>
        Task<IEnumerable<TicketDto>> GetTicketsForUserAsync(User currentUser);

        /// <summary>
        /// Obtiene datos agregados (estadísticas, reportes diarios) para el dashboard de un usuario.
        /// </summary>
        /// <param name="currentUser">El usuario actualmente autenticado.</param>
        /// <returns>Un diccionario con claves string y valores object representando los datos.</returns>
        Task<Dictionary<string, object>> GetDashboardDataAsync(User currentUser);

        /// <summary>
        /// Obtiene un TicketDto específico por ID, validando si el usuario actual tiene permiso para verlo.
        /// </summary>
        /// <param name="id">ID del ticket.</param>
        /// <param name="currentUser">El usuario actualmente autenticado.</param>
        /// <returns>El TicketDto si se encuentra y tiene permiso, o null en caso contrario.</returns>
        Task<TicketDto> GetTicketByIdAsync(int id, User currentUser);

        /// <summary>
        /// Obtiene los datos necesarios para los menús desplegables de Categorías y Analistas.
        /// </summary>
        /// <returns>Una tupla conteniendo listas de SelectListItem.</returns>
        Task<(IEnumerable<SelectListItem> Categories, IEnumerable<SelectListItem> Analysts)> GetDropdownDataAsync();

        /// <summary>
        /// Crea un nuevo ticket en la base de datos.
        /// </summary>
        /// <param name="ticket">La entidad Ticket a crear (mapeada desde un ViewModel).</param>
        /// <param name="creatorUser">El usuario que está creando el ticket.</param>
        /// <returns>Una tupla indicando éxito/fallo, un mensaje de error (si aplica), y el TicketDto creado.</returns>
        Task<(bool Success, string ErrorMessage, TicketDto CreatedTicket)> CreateTicketAsync(Ticket ticket, User creatorUser);

        /// <summary>
        /// Actualiza un ticket existente en la base de datos. Valida permisos internamente.
        /// </summary>
        /// <param name="viewModel">El ViewModel con los datos actualizados del formulario de edición.</param>
        /// <param name="currentUser">El usuario que realiza la actualización.</param>
        /// <returns>Una tupla indicando éxito/fallo y un mensaje de error (si aplica).</returns>
        Task<(bool Success, string ErrorMessage)> UpdateTicketAsync(TicketEditViewModel viewModel, User currentUser); // <-- Firma actualizada

        /// <summary>
        /// Elimina un ticket por ID. Valida permisos internamente.
        /// </summary>
        /// <param name="id">ID del ticket a eliminar.</param>
        /// <param name="currentUser">El usuario que intenta eliminar.</param>
        /// <returns>Una tupla indicando éxito/fallo y un mensaje de error (si aplica).</returns>
        Task<(bool Success, string ErrorMessage)> DeleteTicketAsync(int id, User currentUser);

        /// <summary>
        /// Obtiene la lista de tickets (como DTOs) resueltos hoy por un analista específico.
        /// </summary>
        /// <param name="currentUser">El usuario Analista.</param>
        /// <returns>Una colección de TicketDto.</returns>
        Task<IEnumerable<TicketDto>> GetDailyResolvedTicketsAsync(User currentUser);

        /// <summary>
        /// Verifica si un ticket con el ID especificado existe.
        /// </summary>
        /// <param name="id">ID del ticket.</param>
        /// <returns>True si existe, False si no.</returns>
        Task<bool> TicketExistsAsync(int id);
    }
}
