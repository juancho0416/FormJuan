
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using form.Services;
using System.Collections.Generic;

namespace form.Pages.Admin.Notificaciones
{
    public class IndexModel : PageModel
    {
        public List<NotificacionItem> Items { get; set; } = new();
        private readonly NotificacionService _svc;

        public IndexModel(NotificacionService svc) { _svc = svc; }

        public IActionResult OnGet()
        {
            var correo = HttpContext.Session.GetString("correo");
            if (string.IsNullOrWhiteSpace(correo))
                return RedirectToPage("/Login");

            var rol = _svc.ObtenerRol(correo);
            if (rol != "Admin" && rol != "Revisor")
                return Forbid();

            Items = _svc.ObtenerParaUsuario(correo);
            return Page();
        }

        public IActionResult OnPostLeer(int id)
        {
            var correo = HttpContext.Session.GetString("correo");
            if (string.IsNullOrWhiteSpace(correo))
                return RedirectToPage("/Login");

            var rol = _svc.ObtenerRol(correo);
            if (rol != "Admin" && rol != "Revisor")
                return Forbid();

            _svc.MarcarLeida(id, correo);
            return RedirectToPage();
        }
    }
}
