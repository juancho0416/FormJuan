
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using form.Services;
using System.Collections.Generic;

namespace form.Pages.Notificaciones
{
    public class IndexModel : PageModel
    {
        public List<NotificacionItem> Items { get; set; } = new();

        // (Opcional) contador para mostrar en la vista
        public int NotifCount { get; private set; }

        private readonly NotificacionService _svc;

        public IndexModel(NotificacionService svc)
        {
            _svc = svc;
        }

        public IActionResult OnGet()
        {
            var correo = HttpContext.Session.GetString("correo");
            if (string.IsNullOrWhiteSpace(correo))
                return RedirectToPage("/Login");

            Items = _svc.ObtenerParaUsuario(correo);

            // ✅ Calcular el número de no leídas ANTES del return
            NotifCount = _svc.ContarNoLeidas(correo);

            return Page();
        }

        public IActionResult OnPostLeer(int id)
        {
            var correo = HttpContext.Session.GetString("correo");
            if (string.IsNullOrWhiteSpace(correo))
                return RedirectToPage("/Login");

            _svc.MarcarLeida(id, correo);
            return RedirectToPage();
        }
    }
}
