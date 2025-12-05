using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using form.Services;
namespace form.Pages
{
    public class EditarModel : PageModel
    {

        [BindProperty]
        public FormularioRecord Formulario { get; set; } = new();

        public void OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Formularios WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                Formulario = new FormularioRecord
                {
                    Id = reader.GetInt32(0),
                    Correo = reader.GetString(1),
                    Nombre = reader.GetString(2),
                    RFC = reader.GetString(3),
                    CURP = reader.GetString(4),
                    Folio = reader.GetString(5),
                    Telefono = reader.GetString(6),
                    Calle = reader.GetString(7),
                    Numero = reader.GetString(8),
                    CodigoPostal = reader.GetString(9),
                    Estado = reader.GetString(10),
                    Municipio = reader.GetString(11),
                    RazonSocial = reader.GetString(12),
                    Fecha = reader.GetString(13)
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
                SET Nombre = $nombre,
                    RFC = $rfc,
                    CURP = $curp,
                    Folio = $folio,
                    Telefono = $telefono,
                    Calle = $calle,
                    Numero = $numero,
                    CodigoPostal = $cp,
                    Estado = $estado,
                    Municipio = $municipio,
                    RazonSocial = $razon
                WHERE Id = $id;
            ";
            command.Parameters.AddWithValue("$id", Formulario.Id);
            command.Parameters.AddWithValue("$nombre", Formulario.Nombre ?? "");
            command.Parameters.AddWithValue("$rfc", Formulario.RFC ?? "");
            command.Parameters.AddWithValue("$curp", Formulario.CURP ?? "");
            command.Parameters.AddWithValue("$folio", Formulario.Folio ?? "");
            command.Parameters.AddWithValue("$telefono", Formulario.Telefono ?? "");
            command.Parameters.AddWithValue("$calle", Formulario.Calle ?? "");
            command.Parameters.AddWithValue("$numero", Formulario.Numero ?? "");
            command.Parameters.AddWithValue("$cp", Formulario.CodigoPostal ?? "");
            command.Parameters.AddWithValue("$estado", Formulario.Estado ?? "");
            command.Parameters.AddWithValue("$municipio", Formulario.Municipio ?? "");
            command.Parameters.AddWithValue("$razon", Formulario.RazonSocial ?? "");

            command.ExecuteNonQuery();

            // Auditoría con fecha
            var auditoria = new AuditoriaService();
            var usuario = HttpContext.Session.GetString("correo") ?? "anonimo";
            auditoria.Registrar(usuario, "ACTUALIZAR", "Formulario", Formulario.Id, Formulario.Fecha);

            TempData["Mensaje"] = "Formulario actualizado correctamente.";
            return RedirectToPage("/Historial");
        }
    }

    public class FormularioRecord
    {
        public int Id { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        public string Folio { get; set; }
        public string Telefono { get; set; }
        public string Calle { get; set; }
        public string Numero { get; set; }
        public string CodigoPostal { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
        public string RazonSocial { get; set; }
        public string Fecha { get; set; }
    }
}





