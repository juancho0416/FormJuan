using Microsoft.Data.Sqlite;
using System;

namespace form.Services
{
    public class AuditoriaService
    {
        private readonly string _connectionString = "Data Source=usuarios.db";

        // Sobrecarga: con fecha
        public void Registrar(string usuario, string accion, string entidad, int entidadId, string? fecha = null)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Auditoria (Usuario, Accion, Entidad, EntidadId, Fecha)
                VALUES ($usuario, $accion, $entidad, $entidadId, $fecha);
            ";
            command.Parameters.AddWithValue("$usuario", usuario);
            command.Parameters.AddWithValue("$accion", accion);
            command.Parameters.AddWithValue("$entidad", entidad);
            command.Parameters.AddWithValue("$entidadId", entidadId);
            command.Parameters.AddWithValue("$fecha", fecha ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }
    }
}

