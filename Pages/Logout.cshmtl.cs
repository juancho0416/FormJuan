using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace form.Pages

{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear(); // elimina toda la sesión
            // return RedirectToPage("/Login");
            return Page();
        }
    }
}
