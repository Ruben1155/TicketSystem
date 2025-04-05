// Models/Category.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [Table]

namespace TicketSystem.Models // Asegúrate que el namespace coincida
{
    // Mapea a la tabla 'Categories' en la base de datos.
    [Table("Categories")]
    public class Category
    {
        // Clave Primaria, mapea a la columna CategoryId (Identity)
        [Key]
        public int CategoryId { get; set; }

        // Mapea a la columna Name. Es requerida y tiene un máximo de 100 caracteres.
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [MaxLength(100)]
        [Display(Name = "Nombre Categoría")] // Texto a mostrar en las vistas
        public string Name { get; set; }

        // Propiedad de navegación: Una categoría puede tener muchos tickets.
        // Es virtual para permitir 'lazy loading' si EF Core está configurado para ello.
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
