
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using form.Services;
using System;
using System.Text.Json;

namespace form.Pages
{
    public class EditarUsuarioModel : PageModel
    {
        private readonly AuditoriaService _auditoria;

        public EditarUsuarioModel(AuditoriaService auditoria)
        {
            _auditoria = auditoria;
        }

        [BindProperty]
        public UsuarioRecord Usuario { get; set; } = new();

        [TempData]
        public string? Mensaje { get; set; }

        public void OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nombre, Apellido, Correo, Rol FROM Usuarios WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                Usuario = new UsuarioRecord
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apellido = reader.GetString(2),
                    Correo = reader.GetString(3),
                    Rol = reader.GetString(4)
                };
            }
            else
            {
                Mensaje = "Usuario no encontrado.";
            }
        }

        public IActionResult OnPost()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            // Leer rol y correo actuales desde BD
            string rolActual, correoActual;
            var getCmd = connection.CreateCommand();
            getCmd.CommandText = "SELECT Rol, Correo FROM Usuarios WHERE Id = $id";
            getCmd.Parameters.AddWithValue("$id", Usuario.Id);

            using (var reader = getCmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    Mensaje = "Usuario no encontrado.";
                    return RedirectToPage("/AdminPanel");
                }
                rolActual = reader.GetString(0);
                correoActual = reader.GetString(1);
            }

            var correoSesion = HttpContext.Session.GetString("correo") ?? string.Empty;
            var esMismoUsuario = string.Equals(correoSesion, correoActual, StringComparison.OrdinalIgnoreCase);
            var esAdminActual = string.Equals(rolActual, "Administrador", StringComparison.OrdinalIgnoreCase);
            var nuevoEsAdmin = string.Equals(Usuario.Rol, "Administrador", StringComparison.OrdinalIgnoreCase);

            // Regla: no auto-degradarse
            if (esMismoUsuario && esAdminActual && !nuevoEsAdmin)
            {
                Mensaje = "No puedes quitarte a ti mismo el rol de Administrador.";
                return RedirectToPage("/AdminPanel");
            }

            // Regla: no dejar el sistema sin administradores
            if (esAdminActual && !nuevoEsAdmin)
            {
                var countCmd = connection.CreateCommand();
                countCmd.CommandText = @"
                    SELECT COUNT(*) FROM Usuarios
                    WHERE Rol = 'Administrador' AND Id <> $id;
                ";
                countCmd.Parameters.AddWithValue("$id", Usuario.Id);

                var restantes = Convert.ToInt32(countCmd.ExecuteScalar());
                if (restantes <= 0)
                {
                    Mensaje = "Debe existir al menos un Administrador en el sistema.";
                    return RedirectToPage("/AdminPanel");
                }
            }

            // Actualizar rol
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = "UPDATE Usuarios SET Rol = $rol WHERE Id = $id";
            updateCmd.Parameters.AddWithValue("$rol", Usuario.Rol);
            updateCmd.Parameters.AddWithValue("$id", Usuario.Id);

            var rows = updateCmd.ExecuteNonQuery();

            // Auditoría: cambio de rol con Detalle en JSON (ANTES/DESPUÉS + contexto)
            var adminCorreo = HttpContext.Session.GetString("correo") ?? "admin";
            var detalleJson = JsonSerializer.Serialize(new
            {
                Antes = new { Rol = rolActual, Correo = correoActual },
                Despues = new { Rol = Usuario.Rol, Correo = correoActual },
                QuienCambio = adminCorreo,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                FechaLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            try
            {
                // Nota: requiere que tu AuditoriaService tenga el parámetro `detalle`
                _auditoria.Registrar(
                    usuario: adminCorreo,
                    accion: "ACTUALIZAR_ROL",
                    entidad: "Usuario",
                    entidadId: Usuario.Id,
                    fecha: null,              // usa DateTime.Now por defecto
                    detalle: detalleJson
                );
            }
            catch
            {
                // no bloquear por auditoría
            }

            Mensaje = rows > 0 ? "Rol actualizado correctamente." : "No se pudo actualizar el rol.";
            return RedirectToPage("/AdminPanel");
        }
    }
}
