using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using form.Services;
using System.Threading.Tasks;

namespace form.Pages.Formularios
{
    public class DeleteModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (Id <= 0)
            {
                ErrorMessage = "ID inválido.";
                return Page();
            }

            try
            {
                using var connection = new SqliteConnection("Data Source=usuarios.db");
                connection.Open();

                // Verificar que existe antes de eliminar
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM Formularios WHERE Id = $id;";
                checkCmd.Parameters.AddWithValue("$id", Id);
                var exists = Convert.ToInt32(checkCmd.ExecuteScalar() ?? 0);

                if (exists == 0)
                {
                    ErrorMessage = "El formulario no existe.";
                    return Page();
                }

                // Usar transacción para integridad
                using var tx = connection.BeginTransaction();

                // Eliminar formulario
                using var deleteCmd = connection.CreateCommand();
                deleteCmd.Transaction = tx;
                deleteCmd.CommandText = "DELETE FROM Formularios WHERE Id = $id;";
                deleteCmd.Parameters.AddWithValue("$id", Id);
                var affected = deleteCmd.ExecuteNonQuery();

                if (affected <= 0)
                {
                    tx.Rollback();
                    ErrorMessage = "No se pudo eliminar el formulario.";
                    return Page();
                }

                // Registrar auditoría EN LA MISMA TRANSACCIÓN
                var usuario = HttpContext.Session.GetString("correo") ?? "anonimo";
                using var auditCmd = connection.CreateCommand();
                auditCmd.Transaction = tx;
                auditCmd.CommandText = @"
                    INSERT INTO Auditoria (Usuario, Accion, Entidad, EntidadId, Fecha)
                    VALUES ($usuario, $accion, $entidad, $entidadId, $fecha);
                ";
                auditCmd.Parameters.AddWithValue("$usuario", usuario);
                auditCmd.Parameters.AddWithValue("$accion", "ELIMINAR");
                auditCmd.Parameters.AddWithValue("$entidad", "Formulario");
                auditCmd.Parameters.AddWithValue("$entidadId", Id);
                auditCmd.Parameters.AddWithValue("$fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                var auditAffected = auditCmd.ExecuteNonQuery();
                if (auditAffected <= 0)
                {
                    tx.Rollback();
                    ErrorMessage = "No se pudo registrar la auditoría.";
                    return Page();
                }

                tx.Commit();

                return RedirectToPage("/Formularios/Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error al eliminar: " + ex.Message;
                return Page();
            }
        }
    }
}






