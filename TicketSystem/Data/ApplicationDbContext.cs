// Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity; // Necesario para IdentityRole, etc. si los usas
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystem.Models; // Namespace de tus modelos

namespace TicketSystem.Data // Asegúrate que el namespace coincida
{
    // Hereda de IdentityDbContext especificando NUESTRA clase User extendida.
    // Esto asegura que las tablas de Identity (AspNetUsers) usen nuestra clase User.
    public class ApplicationDbContext : IdentityDbContext<User> // Cambiado de IdentityDbContext a IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSets para nuestras entidades NO relacionadas con Identity ---

        public DbSet<Category> Categories { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        // YA NO NECESITAMOS el DbSet para la tabla 'Users' separada:
        // public DbSet<User> TicketUsers { get; set; } // <-- ELIMINAR ESTA LÍNEA

        // --- Configuraciones Adicionales ---
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // MUY IMPORTANTE llamar a base.OnModelCreating

            // Aquí podrías añadir configuraciones específicas si fueran necesarias,
            // por ejemplo, si las convenciones de EF Core no detectan bien las relaciones
            // con las claves foráneas de tipo string en Ticket. Pero usualmente
            // [ForeignKey] es suficiente.

            // Ejemplo (probablemente no necesario si usas [ForeignKey]):
            // builder.Entity<Ticket>()
            //     .HasOne(t => t.CreatedByUser)
            //     .WithMany(u => u.CreatedTickets)
            //     .HasForeignKey(t => t.CreatedByUserId)
            //     .OnDelete(DeleteBehavior.Restrict); // O la acción que prefieras al borrar usuario

            // builder.Entity<Ticket>()
            //     .HasOne(t => t.AssignedToUser)
            //     .WithMany(u => u.AssignedTickets)
            //     .HasForeignKey(t => t.AssignedToUserId)
            //     .IsRequired(false) // Indicar que es opcional
            //     .OnDelete(DeleteBehavior.Restrict); // O SetNull si prefieres
        }
    }
}
