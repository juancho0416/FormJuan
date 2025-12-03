using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using System.Collections.Generic;

namespace form.Pages.Formularios
{
    public class IndexModel : PageModel
    {
        public List<Formulario> Formularios { get; set; } = new();

        public void OnGet()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Formularios ORDER BY Id DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Formularios.Add(new Formulario
                {
                    Id = reader.GetInt32(0),
                    Area = reader.GetString(1),
                    Empresa = reader.GetString(2),
                    ISO = reader.GetString(3),
                    NOM = reader.GetString(4),
                    Contrato = reader.GetString(5),
                    Solicitud = reader.GetString(6),
                    Requerimiento = reader.GetString(7),
                    Permiso = reader.GetString(8),
                    Peticion = reader.GetString(9),
                    Correo = reader.GetString(10)
                });
            }
        }
    }
}
