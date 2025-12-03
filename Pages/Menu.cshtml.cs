using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Data.Sqlite;
namespace form.Pages
{
    public class MenuModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly EmailService _email;

        public MenuModel(HttpClient httpClient, EmailService email)
        {
            _httpClient = httpClient;
            _email = email;
        }

        public IActionResult OnGet()
        {
            var correo = HttpContext.Session.GetString("correo");
            if (string.IsNullOrEmpty(correo))
            {
                //si no hay sesion redirigir a login
                return RedirectToPage("/Login");
            }

            ViewData["Correo"] = correo;
            return Page();
        }

    }
}