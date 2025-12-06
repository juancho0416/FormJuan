
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using form.Services;
using System.Collections.Generic;
using System;

namespace form.Pages
{
    public class AdminPanelModel : PageModel
    {
        private readonly AuditoriaService _auditoria;

        public AdminPanelModel(AuditoriaService auditoria)
        {
            _auditoria = auditoria;
        }

        public List<UsuarioRecord> Usuarios { get; set; } = new();
        public bool IsAdmin { get; private set; }

        [TempData] public string? StatusMessage { get; set; }

        public void OnGet()
        {
            IsAdmin = HttpContext.Session.GetString("rol") == "Administrador";
            if (!IsAdmin)
            {
                StatusMessage = "Acceso denegado: se requiere rol Administrador.";
                return;
            }

            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Nombre, Apellido, Correo, Rol FROM Usuarios ORDER BY Id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Usuarios.Add(new UsuarioRecord
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Apellido = reader.GetString(2),
                    Correo = reader.GetString(3),
                    Rol = reader.GetString(4)
                });
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            IsAdmin = HttpContext.Session.GetString("rol") == "Administrador";
            if (!IsAdmin)
            {
                StatusMessage = "Acceso denegado: se requiere rol Administrador.";
                return RedirectToPage();
            }

            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();


            // Obtener correo y rol del usuario a eliminar
            string correoTarget = "";
            string rolTarget = "";
            using (var getCmd = connection.CreateCommand())
            {
                getCmd.CommandText = "SELECT Correo, Rol FROM Usuarios WHERE Id = $id;";
                getCmd.Parameters.AddWithValue("$id", id);
                using var reader = getCmd.ExecuteReader();
                if (!reader.Read())
                {
                    StatusMessage = $"Usuario (Id={id}) no encontrado.";
                    return RedirectToPage();
                }
                correoTarget = reader.GetString(0);
                rolTarget = reader.GetString(1);
            }

            // Validación: no permitir borrar administradores
            if (string.Equals(rolTarget, "Administrador", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "No puedes eliminar cuentas con rol Administrador.";
                return RedirectToPage();
            }

            // Validación adicional: no permitir que un usuario se borre a sí mismo (aunque no sea admin)
            var correoSesion = HttpContext.Session.GetString("correo") ?? string.Empty;
            if (string.Equals(correoTarget, correoSesion, StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "No puedes eliminar tu propia cuenta.";
                return RedirectToPage();
            }

            // Si pasa reglas, proceder con borrado seguro (Formularios por Correo + Usuario)
            using var tx = connection.BeginTransaction();
            int rowsAffected = 0;
            try
            {
                using (var delForms = connection.CreateCommand())
                {
                    delForms.Transaction = tx;
                    delForms.CommandText = "DELETE FROM Formularios WHERE Correo = $correo;";
                    delForms.Parameters.AddWithValue("$correo", correoTarget);
                    delForms.ExecuteNonQuery();
                }

                using (var delUsr = connection.CreateCommand())
                {
                    delUsr.Transaction = tx;
                    delUsr.CommandText = "DELETE FROM Usuarios WHERE Id = $id;";
                    delUsr.Parameters.AddWithValue("$id", id);
                    rowsAffected = delUsr.ExecuteNonQuery();
                }

                tx.Commit();

                if (rowsAffected > 0)
                {
                    var adminCorreo = correoSesion;
                    try { _auditoria.Registrar(adminCorreo, "ELIMINAR", "Usuario", id); } catch { }
                    StatusMessage = $"Usuario (Id={id}) eliminado correctamente.";
                }
                else
                {
                    StatusMessage = $"No se pudo eliminar el usuario (Id={id}).";
                }
            }
            catch (SqliteException ex)
            {
                tx.Rollback();
                StatusMessage = $"Error al eliminar: {ex.Message}.";
            }

            return RedirectToPage();
        }
    }
}


