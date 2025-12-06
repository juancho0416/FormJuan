
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using form.Services;
using form.Models;

namespace form.Pages
{
    public class OlvideModel : PageModel
    {
        private readonly PasswordResetEmailService _resetEmail;
        private readonly IConfiguration _config;
        private readonly AuditoriaService _auditoria;
        private readonly AppSettings _appSettings;

        public OlvideModel(
            PasswordResetEmailService resetEmail,
            IConfiguration config,
            AuditoriaService auditoria,
            AppSettings appSettings)
        {
            _resetEmail = resetEmail;
            _config = config;
            _auditoria = auditoria;
            _appSettings = appSettings;
        }

        [BindProperty] public string Correo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                Mensaje = "Escribe tu correo.";
                return Page();
            }

            var correo = Correo.Trim().ToLower();
            _auditoria.Registrar(correo, "IntentoPasswordReset", "Usuario", 0);

            var connStr = _config.GetConnectionString("DefaultConnection") ?? "Data Source=usuarios.db;Cache=Shared";
            using var cn = new SqliteConnection(connStr);
            await cn.OpenAsync();

            // ¿Existe el usuario?
            long userId = 0;
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id FROM Usuarios WHERE Correo = $c LIMIT 1;";
                cmd.Parameters.AddWithValue("$c", correo);
                var obj = await cmd.ExecuteScalarAsync();
                if (obj == null)
                {
                    // No revelar existencia por seguridad
                    Mensaje = "Si el correo existe, recibirás un enlace de restablecimiento.";
                    _auditoria.Registrar(correo, "PasswordResetNoRevelaExistencia", "Usuario", 0);
                    return Page();
                }
                userId = (obj is long l) ? l : Convert.ToInt64(obj);
            }

            // Construir URL base con AppSettings.BaseUrl
            var baseUrl = string.IsNullOrWhiteSpace(_appSettings.BaseUrl)
                ? $"{Request.Scheme}://{Request.Host}"   // fallback
                : _appSettings.BaseUrl;

            var resetPagePath = "/ResetPassword";
            var resetBase = new Uri($"{baseUrl}{resetPagePath}");

            // Enviar correo (token + expiración)
            var (token, expUtc) = await _resetEmail.EnviarResetAsync(
                correoDestino: correo,
                nombreMostrarFrom: "TuApp",
                fromAddress: "onboarding@resend.dev", // cámbialo a tu remitente verificado en producción
                resetLinkBase: resetBase,
                vigencia: TimeSpan.FromMinutes(30)
            );

            // Guardar token y expiración
            using (var up = cn.CreateCommand())
            {
                up.CommandText = @"
UPDATE Usuarios 
SET ResetToken = $t, ResetTokenExpiraUtc = $e
WHERE Id = $id;";
                up.Parameters.AddWithValue("$t", token);
                up.Parameters.AddWithValue("$e", expUtc.ToString("o")); // ISO-8601 UTC
                up.Parameters.AddWithValue("$id", userId);
                await up.ExecuteNonQueryAsync();
            }

            _auditoria.Registrar(correo, "PasswordResetLinkGenerado", "Usuario", (int)userId);

            Mensaje = "Si el correo existe, recibirás un enlace de restablecimiento.";
            return Page();
        }
    }
}
