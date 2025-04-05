// Models/User.cs
using Microsoft.AspNetCore.Identity; // Necesario para IdentityUser
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Para [InverseProperty]
using TicketSystem.Models; // Para UserRole y Ticket

namespace TicketSystem.Models // Asegúrate que el namespace coincida
{
    // Heredamos de IdentityUser para integrar con el sistema de autenticación.
    // IdentityUser ya incluye propiedades como Id (string), UserName, Email, PasswordHash, etc.
    public class User : IdentityUser
    {
        // --- Propiedades Personalizadas ---

        // Añadimos nuestra propiedad Role específica de la aplicación.
        // EF Core (con migraciones) añadirá esta columna a la tabla AspNetUsers.
        public UserRole Role { get; set; }

        // --- Propiedades de Navegación (Igual que antes) ---
        // Siguen siendo útiles para navegar desde un usuario a sus tickets.

        [InverseProperty("CreatedByUser")]
        public virtual ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

        [InverseProperty("AssignedToUser")]
        public virtual ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

        // Nota: Ya no necesitamos UserId (int), Email, PasswordHash, CreatedAt
        // porque vienen de IdentityUser (o son manejadas por Identity).
        // El ID principal ahora es 'Id' (string) heredado de IdentityUser.
    }
}
