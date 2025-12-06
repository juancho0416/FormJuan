using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace form.Pages
{
    public class DetalleUsuarioModel : PageModel
    {
        public UsuarioRecord Usuario { get; set; } = new();
        public List<FormularioRecord> Formularios { get; set; } = new();
        public List<AuditoriaRecord> Acciones { get; set; } = new();

        public void OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            // Obtener datos del usuario
            var cmdUsuario = connection.CreateCommand();
            cmdUsuario.CommandText = "SELECT Id, Nombre, Apellido, Correo, Rol FROM Usuarios WHERE Id = $id";
            cmdUsuario.Parameters.AddWithValue("$id", id);

            using var readerU = cmdUsuario.ExecuteReader();
            if (readerU.Read())
            {
                Usuario = new UsuarioRecord
                {
                    Id = readerU.GetInt32(0),
                    Nombre = readerU.GetString(1),
                    Apellido = readerU.GetString(2),
                    Correo = readerU.GetString(3),
                    Rol = readerU.GetString(4)
                };
            }

            // Obtener formularios del usuario
            var cmdForm = connection.CreateCommand();
            cmdForm.CommandText = "SELECT Id, RazonSocial, Fecha FROM Formularios WHERE Correo = $correo";
            cmdForm.Parameters.AddWithValue("$correo", Usuario.Correo);

            using var readerF = cmdForm.ExecuteReader();
            while (readerF.Read())
            {
                Formularios.Add(new FormularioRecord
                {
                    Id = readerF.GetInt32(0),
                    RazonSocial = readerF.GetString(1),
                    Fecha = readerF.GetString(2)
                });
            }

            // Obtener acciones de auditoría
            var cmdAud = connection.CreateCommand();
            cmdAud.CommandText = "SELECT Accion, Entidad, EntidadId, Fecha FROM Auditoria WHERE Usuario = $usuario";
            cmdAud.Parameters.AddWithValue("$usuario", Usuario.Correo);

            using var readerA = cmdAud.ExecuteReader();
            while (readerA.Read())
            {
                Acciones.Add(new AuditoriaRecord
                {
                    Accion = readerA.GetString(0),
                    Entidad = readerA.GetString(1),
                    EntidadId = readerA.GetInt32(2),
                    Fecha = readerA.GetString(3)
                });
            }
        }
    }

    public class UsuarioRecord
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol { get; set; } = "";

    }

    public class AuditoriaRecord
    {
        public string Accion { get; set; } = "";
        public string Entidad { get; set; } = "";
        public int EntidadId { get; set; }
        public string Fecha { get; set; } = "";
    }
}
