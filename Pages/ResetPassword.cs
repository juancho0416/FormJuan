
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using form.Services;

namespace form.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly AuditoriaService _auditoria;

        public ResetPasswordModel(IConfiguration config, AuditoriaService auditoria)
        {
            _config = config;
            _auditoria = auditoria;
        }

        [BindProperty] public string Token { get; set; } = string.Empty;
        [BindProperty] public string Nueva { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;

        public void OnGet(string token)
        {
            Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(Nueva))
            {
                Error = "Datos incompletos.";
                return Page();
            }

            var connStr = _config.GetConnectionString("DefaultConnection") ?? "Data Source=usuarios.db;Cache=Shared";
            using var cn = new SqliteConnection(connStr);
            await cn.OpenAsync();

            long userId = 0;
            string correo = "";
            DateTime expUtc = DateTime.MinValue;

            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT Id, Correo, ResetTokenExpiraUtc 
FROM Usuarios WHERE ResetToken = $t LIMIT 1;";
                cmd.Parameters.AddWithValue("$t", Token);

                using var rd = await cmd.ExecuteReaderAsync();
                if (!rd.Read())
                {
                    Error = "Token inválido.";
                    _auditoria.Registrar("", "PasswordResetTokenInvalido", "Usuario", 0);
                    return Page();
                }

                userId = rd.GetInt64(0);
                correo = rd.GetString(1);
                expUtc = DateTime.Parse(rd.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind);
            }

            if (DateTime.UtcNow > expUtc)
            {
                Error = "El enlace ha expirado.";
                _auditoria.Registrar(correo, "PasswordResetExpirado", "Usuario", (int)userId);
                return Page();
            }

            // Hash de nueva contraseña (manteniendo tu enfoque actual con SHA256)
            var hashed = HashPassword(Nueva);

            using (var up = cn.CreateCommand())
            {
                up.CommandText = @"
UPDATE Usuarios 
SET Contrasena = $p, ResetToken = NULL, ResetTokenExpiraUtc = NULL
WHERE Id = $id;";
                up.Parameters.AddWithValue("$p", hashed);
                up.Parameters.AddWithValue("$id", userId);
                await up.ExecuteNonQueryAsync();
            }

            _auditoria.Registrar(correo, "PasswordResetExitoso", "Usuario", (int)userId);

            return RedirectToPage("/Login");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }
    }
}
