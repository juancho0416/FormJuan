
using Microsoft.Data.Sqlite;
using System;

namespace form.Services
{
    public class AuditoriaService
    {
        private readonly string _connectionString;

        public AuditoriaService(string connectionString = "Data Source=usuarios.db")
        {
            _connectionString = connectionString;
        }

        // Con soporte para 'detalle' opcional
        public void Registrar(string usuario, string accion, string entidad, int entidadId, string? fecha = null, string? detalle = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Auditoria (Usuario, Accion, Entidad, EntidadId, Fecha, Detalle)
                VALUES ($usuario, $accion, $entidad, $entidadId, $fecha, $detalle);
            ";

            command.Parameters.AddWithValue("$usuario", usuario);
            command.Parameters.AddWithValue("$accion", accion);
            command.Parameters.AddWithValue("$entidad", entidad);
            command.Parameters.AddWithValue("$entidadId", entidadId);
            command.Parameters.AddWithValue("$fecha", fecha ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // ✅ Manejo correcto de NULL para Detalle
            command.Parameters.AddWithValue("$detalle", string.IsNullOrWhiteSpace(detalle) ? (object)DBNull.Value : detalle);

            command.ExecuteNonQuery();
        }
    }

}
