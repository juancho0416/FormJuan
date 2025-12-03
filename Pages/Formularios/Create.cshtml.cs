using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Models;
using form.Services;

namespace form.Pages.Formularios
{

    public class CreateModel : PageModel
    {
        [BindProperty]
        public Formulario Formulario { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
                INSERT INTO Formularios (Area, Empresa, ISO, NOM, Contrato, Solicitud, Requerimiento, Permiso, Peticion, Correo)
                VALUES ($area, $empresa, $iso, $nom, $contrato, $solicitud, $requerimiento, $permiso, $peticion, $correo);
            ";
            insertCmd.Parameters.AddWithValue("$area", Formulario.Area);
            insertCmd.Parameters.AddWithValue("$empresa", Formulario.Empresa);
            insertCmd.Parameters.AddWithValue("$iso", Formulario.ISO);
            insertCmd.Parameters.AddWithValue("$nom", Formulario.NOM);
            insertCmd.Parameters.AddWithValue("$contrato", Formulario.Contrato);
            insertCmd.Parameters.AddWithValue("$solicitud", Formulario.Solicitud);
            insertCmd.Parameters.AddWithValue("$requerimiento", Formulario.Requerimiento);
            insertCmd.Parameters.AddWithValue("$permiso", Formulario.Permiso);
            insertCmd.Parameters.AddWithValue("$peticion", Formulario.Peticion);
            insertCmd.Parameters.AddWithValue("$correo", Formulario.Correo);

            insertCmd.ExecuteNonQuery();

            // obtener el id insertado (forma compatible)
            using var lastIdCmd = connection.CreateCommand();
            lastIdCmd.CommandText = "SELECT last_insert_rowid();";
            var lastIdObj = lastIdCmd.ExecuteScalar();
            long lastInsertId = lastIdObj is long l ? l : Convert.ToInt64(lastIdObj);

            // si necesitas int:
            int insertedId = (int)lastInsertId;

            // Auditoría
            // Auditoría
            var auditoria = new AuditoriaService();
            var usuario = User.Identity.IsAuthenticated ? User.Identity.Name : "anonimo";
            auditoria.Registrar(usuario, "CREAR", "Formulario", insertedId);

            return RedirectToPage("/Formularios/Index");
        }
    }

}
