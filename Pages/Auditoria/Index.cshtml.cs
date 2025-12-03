using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using System.Collections.Generic;

// evita la ambigüedad con el namespace form.Pages.Auditoria
using AuditoriaModel = form.Models.Auditoria;

namespace form.Pages.Auditoria
{
    public class IndexModel : PageModel
    {
        public List<AuditoriaModel> Registros { get; set; } = new();

        public void OnGet()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Auditoria ORDER BY Fecha DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Registros.Add(new AuditoriaModel
                {
                    Id = reader.GetInt32(0),
                    Usuario = reader.GetString(1),
                    Accion = reader.GetString(2),
                    Entidad = reader.GetString(3),
                    EntidadId = reader.GetInt32(4),
                    Fecha = reader.GetString(5)
                });
            }
        }
    }
}
