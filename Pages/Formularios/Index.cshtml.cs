using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using System.Collections.Generic;

namespace form.Pages.Formularios
{
    public class IndexModel : PageModel
    {
        public List<FormularioRecord> Formularios { get; set; } = new();

        public void OnGet()
        {
            string correo = HttpContext.Session.GetString("correo") ?? string.Empty;

            if (string.IsNullOrEmpty(correo))
                return;

            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, Nombre, RFC, CURP, Folio, Telefono, Fecha, CodigoPostal,Correo
                                    FROM Formularios 
                                    WHERE Correo = $correo
                                    ORDER BY Fecha DESC";
            command.Parameters.AddWithValue("$correo", correo);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Formularios.Add(new FormularioRecord
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    RFC = reader.GetString(2),
                    CURP = reader.GetString(3),
                    Folio = reader.GetString(4),
                    Telefono = reader.GetString(5),
                    Fecha = reader.GetString(6),
                    CodigoPostal = reader.GetString(7),
                    Correo = reader.GetString(8)
                });
            }
        }

        public class FormularioRecord
        {

            public int Id { get; set; }
            public string Nombre { get; set; }
            public string RFC { get; set; }
            public string CURP { get; set; }
            public string Folio { get; set; }
            public string Telefono { get; set; }
            public string Fecha { get; set; }
            public string CodigoPostal { get; set; }
            public string Correo { get; set; }
        }
    }
}
