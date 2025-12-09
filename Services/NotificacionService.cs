
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace form.Services
{

    public class NotificacionItem
    {
        public int Id { get; set; }
        public string? ParaCorreo { get; set; }
        public string? ParaRol { get; set; }
        public string Titulo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Tipo { get; set; } = "INFO";
        public string? Link { get; set; }
        public string? CreadaEn { get; set; }
        public bool Leida { get; set; }

        // Alias para compatibilidad con la vista existente
        public string? LinkIr => Link;
    }

    public class NotificacionService
    {
        private readonly string _connString;
        public NotificacionService(string connString = "Data Source=usuarios.db")
        {
            _connString = connString;
        }

        private void EnableFK(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }

        public int CrearParaUsuario(string correo, string titulo, string mensaje, string tipo = "INFO", string? link = null)
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open(); EnableFK(conn);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Notificaciones (ParaCorreo, Titulo, Mensaje, Tipo, Link)
                VALUES ($correo, $titulo, $mensaje, $tipo, $link);
            ";
            cmd.Parameters.AddWithValue("$correo", correo);
            cmd.Parameters.AddWithValue("$titulo", titulo);
            cmd.Parameters.AddWithValue("$mensaje", mensaje);
            cmd.Parameters.AddWithValue("$tipo", tipo);
            cmd.Parameters.AddWithValue("$link", (object?)link ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            // devolver id insertado
            using var idCmd = conn.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public int CrearParaRol(string rol, string titulo, string mensaje, string tipo = "INFO", string? link = null)
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open(); EnableFK(conn);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Notificaciones (ParaRol, Titulo, Mensaje, Tipo, Link)
                VALUES ($rol, $titulo, $mensaje, $tipo, $link);
            ";
            cmd.Parameters.AddWithValue("$rol", rol);
            cmd.Parameters.AddWithValue("$titulo", titulo);
            cmd.Parameters.AddWithValue("$mensaje", mensaje);
            cmd.Parameters.AddWithValue("$tipo", tipo);
            cmd.Parameters.AddWithValue("$link", (object?)link ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            using var idCmd = conn.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public List<NotificacionItem> ObtenerParaUsuario(string correo)
        {
            // Obtener rol del usuario
            string rol = ObtenerRol(correo);

            var lista = new List<NotificacionItem>();
            using var conn = new SqliteConnection(_connString);
            conn.Open(); EnableFK(conn);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT n.Id, n.ParaCorreo, n.ParaRol, n.Titulo, n.Mensaje, n.Tipo, n.Link, n.CreadaEn,
                       CASE WHEN l.NotificacionId IS NULL THEN 0 ELSE 1 END AS Leida
                FROM Notificaciones n
                LEFT JOIN NotificacionLecturas l
                       ON l.NotificacionId = n.Id AND l.Correo = $correo
                WHERE (n.ParaCorreo = $correo) OR (n.ParaRol = $rol)
                ORDER BY n.CreadaEn DESC, n.Id DESC;
            ";
            cmd.Parameters.AddWithValue("$correo", correo);
            cmd.Parameters.AddWithValue("$rol", rol);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                lista.Add(new NotificacionItem
                {
                    Id = r.GetInt32(0),
                    ParaCorreo = r["ParaCorreo"]?.ToString(),
                    ParaRol = r["ParaRol"]?.ToString(),
                    Titulo = r["Titulo"]?.ToString() ?? "",
                    Mensaje = r["Mensaje"]?.ToString() ?? "",
                    Tipo = r["Tipo"]?.ToString() ?? "INFO",
                    Link = r["Link"]?.ToString(),
                    CreadaEn = r["CreadaEn"]?.ToString(),
                    Leida = Convert.ToInt32(r["Leida"]) == 1
                });
            }
            return lista;
        }

        public void MarcarLeida(int notificacionId, string correo)
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open(); EnableFK(conn);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT OR IGNORE INTO NotificacionLecturas (NotificacionId, Correo)
                VALUES ($id, $correo);
            ";
            cmd.Parameters.AddWithValue("$id", notificacionId);
            cmd.Parameters.AddWithValue("$correo", correo);
            cmd.ExecuteNonQuery();
        }

        public int ContarNoLeidas(string correo)
        {
            string rol = ObtenerRol(correo);
            using var conn = new SqliteConnection(_connString);
            conn.Open(); EnableFK(conn);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COUNT(*)
                FROM Notificaciones n
                LEFT JOIN NotificacionLecturas l
                       ON l.NotificacionId = n.Id AND l.Correo = $correo
                WHERE ((n.ParaCorreo = $correo) OR (n.ParaRol = $rol))
                  AND l.NotificacionId IS NULL;
            ";
            cmd.Parameters.AddWithValue("$correo", correo);
            cmd.Parameters.AddWithValue("$rol", rol);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public string ObtenerRol(string correo)
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Rol FROM Usuarios WHERE Correo = $c LIMIT 1;";
            cmd.Parameters.AddWithValue("$c", correo);
            var rolObj = cmd.ExecuteScalar();
            var rol = rolObj?.ToString() ?? "Usuario";
            return string.IsNullOrWhiteSpace(rol) ? "Usuario" : rol;
        }
    }
}
