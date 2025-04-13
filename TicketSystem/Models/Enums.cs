// Models/Enums.cs
// Este archivo define las enumeraciones usadas en los modelos,
// proporcionando nombres descriptivos para los códigos numéricos almacenados en la base de datos.

namespace TicketSystem.Models // Asegúrate que el namespace coincida con tu proyecto
{
    // Define los roles de usuario posibles, coincidiendo con los valores en la tabla Users
    public enum UserRole
    {
        Support = 1, // Compañero de Soporte
        Analyst = 2, // Analista
        Admin = 3    // <-- AÑADIDO: Administrador
    }

    // Define los niveles de Urgencia para un Ticket
    public enum TicketUrgency
    {
        Baja = 1,
        Media = 2,
        Alta = 3
    }

    // Define los niveles de Importancia para un Ticket
    public enum TicketImportance
    {
        Baja = 1,
        Media = 2,
        Alta = 3
    }

    // Define los posibles estados de un Ticket
    // Asegúrate que estos valores coincidan con tu lógica de negocio y la tabla Tickets
    public enum TicketStatus
    {
        Creado = 1,     // Ticket Recién Creado
        Pendiente = 2,  // Ticket sin asignar
        Asignado = 3,   // Ticket asignado a un Analista
        EnProgreso = 4, // Ticket en proceso de resolución
        Resuelto = 5,   // Ticket resuelto
        Cerrado = 6     // Opcional, si necesitas un estado final después de resuelto
    }
}
