
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using form.Services;
using System.Text.Json;

// <-- importante
// : para usar tu AuditoriaService

namespace form.Pages
{
    public class RegistroModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly EmailService _email;
        private readonly AuditoriaService _auditoria; // <-- tu servicio

        public RegistroModel(IConfiguration config, EmailService email, AuditoriaService auditoria)
        {
            _config = config;
            _email = email;
            _auditoria = auditoria;
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

            // Auditoría: intento de registro (EntidadId = 0 porque aún no existe)
            _auditoria.Registrar(correoNormalizado, "IntentoRegistro", "Usuario", 0);

            // Usa la conexión centralizada si la tienes en appsettings, si no, usa por defecto
            var connStr = _config.GetConnectionString("Default") ?? "Data Source=usuarios.db";
            using var connection = new SqliteConnection(connStr);
            await connection.OpenAsync();

            // Verificar duplicados
            using (var check = connection.CreateCommand())
            {
                check.CommandText = "SELECT COUNT(*) FROM Usuarios WHERE Correo = $correo";
                check.Parameters.AddWithValue("$correo", correoNormalizado);

                long count = (long)(await check.ExecuteScalarAsync());

                if (count > 0)
                {
                    ErrorMessage = "El correo ya está registrado.";

                    // Auditoría: correo duplicado
                    _auditoria.Registrar(correoNormalizado, "CorreoDuplicado", "Usuario", 0);

                    return Page();
                }
            }

            string hashedPassword = HashPassword(Input.Contraseña);
            string token = Guid.NewGuid().ToString();

            long nuevoUsuarioId = 0;

            // Insertar usuario y recuperar ID
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Usuarios (Nombre, Apellido, Correo, Contrasena, TokenConfirmacion, Confirmado)
                    VALUES ($nombre, $apellido, $correo, $contrasena, $token, 0);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("$nombre", Input.Nombre);
                command.Parameters.AddWithValue("$apellido", Input.Apellido);
                command.Parameters.AddWithValue("$correo", correoNormalizado);
                command.Parameters.AddWithValue("$contrasena", hashedPassword);
                command.Parameters.AddWithValue("$token", token);

                object result = await command.ExecuteScalarAsync();
                nuevoUsuarioId = (result is long l) ? l : Convert.ToInt64(result);
            }

            // Auditoría: usuario creado
            _auditoria.Registrar(correoNormalizado, "UsuarioCreado", "Usuario", (int)nuevoUsuarioId);

            try
            {
                await _email.EnviarCorreoConfirmacion(correoNormalizado, token);
                Console.WriteLine("Correo enviado correctamente");

                // Auditoría: correo de confirmación enviado
                _auditoria.Registrar(correoNormalizado, "CorreoConfirmacionEnviado", "Usuario", (int)nuevoUsuarioId);

                return Redirect("/ConfirmacionEnviada");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error enviando correo: " + ex.Message);

                // Auditoría: error enviando correo (guardamos el mensaje en la acción)
                _auditoria.Registrar(
                    usuario: correoNormalizado,
                    accion: $"ErrorEnvioCorreo: {ex.Message}",
                    entidad: "Usuario",
                    entidadId: (int)nuevoUsuarioId
                );

                ErrorMessage = "Ocurrió un problema enviando el correo de confirmación. Intente más tarde.";
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
}
