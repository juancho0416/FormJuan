using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;

namespace form.Pages.Formularios
{
    public class DetailsModel : PageModel
    {
        public Formulario Formulario { get; set; }

        public void OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Formularios WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                Formulario = new Formulario
                {
                    Id = reader.GetInt32(0),
                    Area = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Empresa = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ISO = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    NOM = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Contrato = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Solicitud = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Requerimiento = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Permiso = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Peticion = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    Correo = reader.IsDBNull(10) ? "" : reader.GetString(10)
                };
            }
        }
    }
}
