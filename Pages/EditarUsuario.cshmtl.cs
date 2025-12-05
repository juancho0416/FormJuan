using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;

namespace form.Pages
{
    public class EditarUsuarioModel : PageModel
    {
        [BindProperty]
        public UsuarioRecord Usuario { get; set; } = new();
        public List<UsuarioRecord> Usuarios { get; set; } = new();
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
        }

        public IActionResult OnPost()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Usuarios SET Rol = $rol WHERE Id = $id";
            command.Parameters.AddWithValue("$rol", Usuario.Rol);
            command.Parameters.AddWithValue("$id", Usuario.Id);

            command.ExecuteNonQuery();

            TempData["Mensaje"] = "Rol actualizado correctamente.";
            return RedirectToPage("/AdminPanel");
        }




    }
}
