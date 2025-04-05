// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using TicketSystem.Models; // Necesario para User y UserRole

namespace TicketSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // <-- AÑADIDO: Para gestionar roles de Identity
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender; // Opcional, si configuras envío de email

        // Constructor actualizado para inyectar RoleManager
        public RegisterModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, // <-- AÑADIDO
            IUserStore<User> userStore,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender) // <-- Quita emailSender si no lo usas
        {
            _userManager = userManager;
            _roleManager = roleManager; // <-- AÑADIDO
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender; // <-- Quita emailSender si no lo usas
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // Clase interna que define los campos del formulario de registro
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            // --- PROPIEDAD AÑADIDA PARA EL ROL ---
            [Required(ErrorMessage = "Debe seleccionar un rol.")]
            [Display(Name = "Rol de Usuario")]
            public UserRole Role { get; set; } // Usa nuestra enumeración UserRole
            // --- FIN PROPIEDAD AÑADIDA ---
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Verifica si los datos del formulario son válidos según las anotaciones en InputModel
            if (ModelState.IsValid)
            {
                // Crea una instancia de nuestra clase User extendida
                var user = CreateUser();

                // Asigna el Email y UserName (usualmente el mismo que el email)
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // --- ASIGNACIÓN DE ROL PERSONALIZADO ---
                // Asigna el rol seleccionado en el formulario a nuestra propiedad Role
                user.Role = Input.Role;
                // --- FIN ASIGNACIÓN ROL PERSONALIZADO ---

                // Intenta crear el usuario en la tabla AspNetUsers con su contraseña hasheada
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded) // Si la creación del usuario fue exitosa...
                {
                    _logger.LogInformation("Usuario creó una nueva cuenta con contraseña.");

                    // --- ASIGNACIÓN DE ROL DE IDENTITY (RECOMENDADO) ---
                    // Asigna el rol correspondiente ("Support" o "Analyst") al sistema de roles de Identity.
                    // Esto es necesario para que [Authorize(Roles = "...")] funcione correctamente.
                    var roleName = Input.Role.ToString(); // Convierte el valor del enum (Support/Analyst) a string
                    try
                    {
                        // Verifica si el rol con ese nombre existe en la tabla AspNetRoles
                        if (await _roleManager.RoleExistsAsync(roleName))
                        {
                            // Si existe, añade al usuario a ese rol (crea registro en AspNetUserRoles)
                            await _userManager.AddToRoleAsync(user, roleName);
                            _logger.LogInformation($"Usuario '{user.Email}' asignado al rol '{roleName}'.");
                        }
                        else
                        {
                            // Si el rol no existe, registra una advertencia.
                            // Idealmente, los roles "Support" y "Analyst" ya deberían existir (creados con SQL o programáticamente).
                            _logger.LogWarning($"El Rol '{roleName}' no existe en la base de datos. Se omitió la asignación de rol de Identity para el usuario '{user.Email}'.");
                            // Considera añadir lógica aquí para crear el rol si no existe, si es necesario.
                        }
                    }
                    catch (Exception ex)
                    {
                        // Captura cualquier error durante la asignación de rol para que no detenga el registro
                        _logger.LogError(ex, $"Error asignando el rol '{roleName}' de Identity al usuario '{user.Email}'.");
                        // Podrías añadir un error a ModelState aquí si quieres informar al usuario.
                        // ModelState.AddModelError(string.Empty, "Ocurrió un error al asignar el rol de usuario.");
                    }
                    // --- FIN ASIGNACIÓN ROL IDENTITY ---


                    var userId = await _userManager.GetUserIdAsync(user);

                    // --- Código para confirmación de email (opcional) ---
                    // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    // var callbackUrl = Url.Page(
                    //     "/Account/ConfirmEmail",
                    //     pageHandler: null,
                    //     values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    //     protocol: Request.Scheme);

                    // await _emailSender.SendEmailAsync(Input.Email, "Confirma tu email",
                    //     $"Por favor confirma tu cuenta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>haciendo clic aquí</a>.");

                    // if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    // {
                    //     return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    // }
                    // else // Si no requiere confirmación, inicia sesión directamente
                    // {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                    // }
                    // --- Fin código confirmación email ---
                }

                // Si result NO fue Succeeded, añade los errores al ModelState para mostrarlos en el formulario
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Si ModelState no es válido (o la creación falló), vuelve a mostrar el formulario
            return Page();
        }

        // Método helper para crear la instancia del usuario
        private User CreateUser()
        {
            try
            {
                // Devuelve una instancia de TU clase User extendida
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"No se puede crear una instancia de '{nameof(User)}'. " +
                    $"Asegúrate que '{nameof(User)}' no sea una clase abstracta y tenga un constructor sin parámetros, o alternativamente " +
                    $"sobrescribe la página de registro en /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Método helper para obtener el UserStore con soporte de Email
        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("La UI por defecto requiere un user store con soporte de email.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
