using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using form.Services;

namespace form.Pages.Formularios
{
    public class EditModel : PageModel
    {
        [BindProperty]
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

        public IActionResult OnPost()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Formularios
                SET Area=$area, Empresa=$empresa, ISO=$iso, NOM=$nom, Contrato=$contrato,
                    Solicitud=$solicitud, Requerimiento=$requerimiento, Permiso=$permiso,
                    Peticion=$peticion, Correo=$correo
                WHERE Id=$id;
            ";
            command.Parameters.AddWithValue("$id", Formulario.Id);
            command.Parameters.AddWithValue("$area", Formulario.Area);
            command.Parameters.AddWithValue("$empresa", Formulario.Empresa);
            command.Parameters.AddWithValue("$iso", Formulario.ISO);
            command.Parameters.AddWithValue("$nom", Formulario.NOM);
            command.Parameters.AddWithValue("$contrato", Formulario.Contrato);
            command.Parameters.AddWithValue("$solicitud", Formulario.Solicitud);
            command.Parameters.AddWithValue("$requerimiento", Formulario.Requerimiento);
            command.Parameters.AddWithValue("$permiso", Formulario.Permiso);
            command.Parameters.AddWithValue("$peticion", Formulario.Peticion);
            command.Parameters.AddWithValue("$correo", Formulario.Correo);

            command.ExecuteNonQuery();

            // Auditoría
            var auditoria = new AuditoriaService();
            var usuario = User.Identity.IsAuthenticated ? User.Identity.Name : "anonimo";
            auditoria.Registrar(usuario, "ACTUALIZAR", "Formulario", Formulario.Id);

            return RedirectToPage("/Formularios/Index");
        }
    }
}
