using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
// Quita using TicketSystem.Models; si no usas ErrorViewModel

namespace TicketSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Modificado: Redirige a la página de Login de Identity
        public IActionResult Index()
        {
            // Redirige al área de Identity, página de Account/Login
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // Eliminado: Ya no necesitamos la acción Privacy
        // public IActionResult Privacy()
        // {
        //     return View();
        // }

        // Mantenido (Opcional): Puedes mantener esto si quieres una página de error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Necesitarías crear un ErrorViewModel si lo eliminas de Models
            // o simplemente devolver una vista básica de error.
            // return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            // O una vista simple:
            return View(); // Asegúrate de tener Views/Shared/Error.cshtml
        }
    }
}

// Si mantienes Error(), asegúrate que ErrorViewModel exista o elimina su uso.
// Puedes borrar la clase ErrorViewModel de Models si no la usas.
// namespace TicketSystem.Models
// {
//     public class ErrorViewModel
//     {
//         public string? RequestId { get; set; }
//         public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
//     }
// }
