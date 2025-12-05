using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace form.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new LoginInput();

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet() { }

    public IActionResult OnPost()
    {
        string correoNormalizado = Input.Email.Trim().ToLower();

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Contrasena, Confirmado, Rol
        FROM Usuarios
        WHERE Correo = $correo;
    ";
        command.Parameters.AddWithValue("$correo", correoNormalizado);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            ErrorMessage = "El correo no está registrado";
            return Page();
        }

        string storedHash = reader.GetString(0);
        int confirmado = reader.GetInt32(1);
        string rol = reader.GetString(2);

        if (confirmado == 0)
        {
            ErrorMessage = "Debes confirmar tu correo antes de iniciar sesión.";
            return Page();
        }

        string enteredHash = HashPassword(Input.Password);

        if (storedHash != enteredHash)
        {
            ErrorMessage = "Contraseña incorrecta";
            return Page();
        }

        // Guardar en sesión
        HttpContext.Session.SetString("correo", correoNormalizado);
        HttpContext.Session.SetString("rol", rol);

        // Redirigir según rol
        if (rol == "Administrador")
        {
            return RedirectToPage("AdminPanel");
        }
        else
        {
            return RedirectToPage("Menu");
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "Correo electronico requerido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contraseña requerida")]
        public string Password { get; set; } = string.Empty;
    }
}
