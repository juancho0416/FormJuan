using Microsoft.Data.Sqlite;

namespace form.Services
{
    public class AuditoriaService
    {
        private readonly string _connectionString = "Data Source=usuarios.db";

        public void Registrar(string usuario, string accion, string entidad, int entidadId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Auditoria (Usuario, Accion, Entidad, EntidadId)
                VALUES ($usuario, $accion, $entidad, $entidadId);
            ";
            command.Parameters.AddWithValue("$usuario", usuario);
            command.Parameters.AddWithValue("$accion", accion);
            command.Parameters.AddWithValue("$entidad", entidad);
            command.Parameters.AddWithValue("$entidadId", entidadId);

            command.ExecuteNonQuery();

        }
    }
}
