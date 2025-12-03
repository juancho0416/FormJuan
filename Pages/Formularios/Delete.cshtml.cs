using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Services;

namespace form.Pages.Formularios
{
    public class DeleteModel : PageModel
    {
        public void OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Formularios WHERE Id = $id;";
            command.Parameters.AddWithValue("$id", id);

            command.ExecuteNonQuery();

            // Auditoría
            var auditoria = new AuditoriaService();
            auditoria.Registrar("admin@dominio.com", "ELIMINAR", "Formulario", id);
        }
    }
}
