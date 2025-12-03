using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace form.Pages;

public class ConfirmarModel : PageModel
{
    public string Mensaje { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; }

    public IActionResult OnGet(string token)
    {
        token = token?.Trim();

        // LOG TEMPORAL
        Console.WriteLine($"TOKEN EN LA URL: '{token}'");

        if (string.IsNullOrEmpty(token))
        {
            Mensaje = "Token inválido (no llegó token).";
            return Page();
        }

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
        SELECT COUNT(*) FROM Usuarios
        WHERE TokenConfirmacion = $token;
    ";
        command.Parameters.AddWithValue("$token", token);

        long count = (long)command.ExecuteScalar();

        if (count == 0)
        {
            Mensaje = $"Token no válido en BD. Token recibido: '{token}'.";
            return Page();
        }

        var update = connection.CreateCommand();
        update.CommandText = @"
        UPDATE Usuarios
        SET Confirmado = 1
        WHERE TokenConfirmacion = $token;
    ";
        update.Parameters.AddWithValue("$token", token);
        update.ExecuteNonQuery();

        Mensaje = "Tu correo ha sido confirmado. Ya puedes iniciar sesión.";
        return RedirectToPage("/login");
        return Page();
    }

}