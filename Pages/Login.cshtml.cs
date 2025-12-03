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
        // Normalizar correo
        string correoNormalizado = Input.Email.Trim().ToLower();

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        // 1. Verificar si el correo existe
        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT Contrasena, Confirmado
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

        //  2. Verificar si el correo está confirmado
        if (confirmado == 0)
        {
            ErrorMessage = "Debes confirmar tu correo antes de iniciar sesión.";
            return Page();
        }

        //  3. Verificar contraseña
        string enteredHash = HashPassword(Input.Password);

        if (storedHash != enteredHash)
        {
            ErrorMessage = "Contraseña incorrecta";
            return Page();
        }

        //  4. Guardar correo en sesión SOLO si todo es correcto
        HttpContext.Session.SetString("correo", correoNormalizado);


        //  5. Redirigir al usuario
        return RedirectToPage("Menu");
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
