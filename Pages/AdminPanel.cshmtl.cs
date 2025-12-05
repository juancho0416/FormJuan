using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;

namespace form.Pages
{
    public class AdminPanelModel : PageModel
    {

        public List<UsuarioRecord> Usuarios { get; set; } = new();

        public void OnGet()
        {
            // Verificar rol
            if (HttpContext.Session.GetString("rol") != "Administrador")
            {
                return; // No carga nada si no es admin
            }

            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nombre, Apellido, Correo, Rol FROM Usuarios";

            using var reader = command.ExecuteReader();
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





    }
}
