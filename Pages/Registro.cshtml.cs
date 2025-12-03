using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace form.Pages;

public class RegistroModel : PageModel
{
    private readonly IConfiguration _config;
    private readonly EmailService _email;

    public RegistroModel(IConfiguration config, EmailService email)
    {
        _config = config;
        _email = email;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ErrorMessage { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        Console.WriteLine("ModelState válido");

        string correoNormalizado = Input.Correo.Trim().ToLower();

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Usuarios WHERE Correo = $correo";
        check.Parameters.AddWithValue("$correo", correoNormalizado);

        long count = (long)check.ExecuteScalar();

        if (count > 0)
        {
            ErrorMessage = "El correo ya está registrado.";
            return Page();
        }

        string hashedPassword = HashPassword(Input.Contraseña);
        string token = Guid.NewGuid().ToString();

        var command = connection.CreateCommand();
        command.CommandText = @"
        INSERT INTO Usuarios (Nombre, Apellido, Correo, Contrasena, TokenConfirmacion, Confirmado)
        VALUES ($nombre, $apellido, $correo, $contrasena, $token, 0);
    ";
        command.Parameters.AddWithValue("$nombre", Input.Nombre);
        command.Parameters.AddWithValue("$apellido", Input.Apellido);
        command.Parameters.AddWithValue("$correo", correoNormalizado);
        command.Parameters.AddWithValue("$contrasena", hashedPassword);
        command.Parameters.AddWithValue("$token", token);
        command.ExecuteNonQuery();

        try
        {
            await _email.EnviarCorreoConfirmacion(correoNormalizado, token);
            Console.WriteLine("Correo enviado correctamente");
            return Redirect("/ConfirmacionEnviada");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error enviando correo: " + ex.Message);
            return Page();
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public class InputModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;
    }
}