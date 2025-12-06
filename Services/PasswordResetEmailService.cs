
using Resend;
using System.Security.Cryptography;
using System.Text;

namespace form.Services
{
    /// <summary>
    /// Servicio exclusivo para correos de restablecimiento de contraseña usando Resend.
    /// Convive con tu EmailService sin conflicto.
    /// </summary>
    public class PasswordResetEmailService
    {
        private readonly IResend _resend;
        private readonly AuditoriaService _auditoria;

        public PasswordResetEmailService(IResend resend, AuditoriaService auditoria)
        {
            _resend = resend;
            _auditoria = auditoria;
        }

        /// <summary>
        /// Envía el correo de restablecimiento y retorna (token, expiraUtc).
        /// Debes persistir token/expiración en tu tabla Usuarios después de llamar a este método.
        /// </summary>
        public async Task<(string token, DateTime expiraUtc)> EnviarResetAsync(
            string correoDestino,
            string nombreMostrarFrom,
            string fromAddress,
            Uri resetLinkBase,           // ej. https://tu-base-url/ResetPassword
            TimeSpan? vigencia = null,   // p.ej. 30 min
            CancellationToken ct = default)
        {
            var token = GenerarTokenSeguro(); // base64url
            var expiraUtc = DateTime.UtcNow.Add(vigencia ?? TimeSpan.FromMinutes(30));

            // Componer URL final con token
            var builder = new UriBuilder(resetLinkBase);
            var query = $"token={Uri.EscapeDataString(token)}";
            builder.Query = string.IsNullOrEmpty(builder.Query) ? query : builder.Query.TrimStart('?') + "&" + query;
            var resetUrl = builder.Uri.ToString();

            var html = RenderHtmlReset(correoDestino, resetUrl, expiraUtc);

            Exception? last = null;
            for (int intento = 1; intento <= 3; intento++)
            {
                try
                {
                    var msg = new EmailMessage
                    {
                        From = $"{nombreMostrarFrom} <{fromAddress}>",
                        Subject = "Restablece tu contraseña",
                        HtmlBody = html,
                    };
                    msg.To.Add(correoDestino);

                    var resp = await _resend.EmailSendAsync(msg, ct);

                    // ✅ El ID del envío viene en Content (Guid), no en Id
                    Console.WriteLine($"[Resend] Email ID: {resp?.Content}");

                    _auditoria.Registrar(correoDestino, "PasswordResetCorreoEnviado", "Usuario", 0);
                    return (token, expiraUtc);
                }
                catch (TaskCanceledException ex)
                {
                    last = ex;
                    Console.WriteLine($"[Resend] Timeout: {ex.Message}");
                    _auditoria.Registrar(correoDestino, $"PasswordResetTimeoutIntento{intento}", "Usuario", 0);
                }
                catch (Exception ex)
                {
                    last = ex;
                    Console.WriteLine($"[Resend] Error: {ex.GetType().Name} - {ex.Message}");
                    _auditoria.Registrar(correoDestino, $"PasswordResetErrorIntento{intento}: {ex.Message}", "Usuario", 0);
                }
                await Task.Delay(TimeSpan.FromSeconds(intento), ct); // backoff 1s, 2s
            }

            throw new InvalidOperationException("No se pudo enviar el correo de restablecimiento.", last);
        }

        private static string GenerarTokenSeguro(int bytes = 32)
        {
            var buffer = new byte[bytes];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToBase64String(buffer).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private static string RenderHtmlReset(string correoDestino, string resetUrl, DateTime expiraUtc)
        {
            var tzCdmx = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
            var expiraLocal = TimeZoneInfo.ConvertTimeFromUtc(expiraUtc, tzCdmx);

            var sb = new StringBuilder();
            sb.Append($@"
<!doctype html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <title>Restablecer contraseña</title>
  <style>
    body{{font-family:Arial,sans-serif;background:#f7f7f7;margin:0;padding:24px}}
    .card{{max-width:560px;margin:auto;background:#fff;border-radius:12px;padding:24px}}
    .btn{{display:inline-block;padding:12px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:8px}}
    p{{color:#333;line-height:1.5}}
    small{{color:#666}}
  </style>
</head>
<body>
  <div class=""card"">
    <h2>Restablecer tu contraseña</h2>
    <p>Recibimos una solicitud para restablecer la contraseña de <strong>{System.Net.WebUtility.HtmlEncode(correoDestino)}</strong>.</p>
    <p>Haz clic en el botón para continuar:</p>
    <p><a class=""btn"" href=""{System.Net.WebUtility.HtmlEncode(resetUrl)}"">Restablecer contraseña</a></p>
    <p>Este enlace vence el <strong>{expiraLocal:yyyy-MM-dd HH:mm}</strong> (hora CDMX).</p>
    <small>Si no solicitaste este cambio, ignora este correo.</small>
  </div>
</body>
</html>");
            return sb.ToString();
        }
    }
}
