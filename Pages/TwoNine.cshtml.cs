using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace form.Pages
{
    public class TwoNineModel : PageModel
    {
        private readonly IWebHostEnvironment _env;
        public TwoNineModel(IWebHostEnvironment env) => _env = env;

        [BindProperty]
        public IFormFile? Archivo { get; set; }

        public string? TipoFormulario { get; set; }

        public void OnGet(string? tipo)
        {
            TipoFormulario = tipo;
            // usar TipoFormulario para adaptar la UI o cargar plantilla
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Archivo == null || Archivo.Length == 0)
            {
                TempData["UploadMessage"] = "Seleccione un archivo.";
                TempData["ShowUploadModal"] = true;
                return Page();
            }

            var ext = Path.GetExtension(Archivo.FileName)?.ToLowerInvariant();
            var permitidos = new[] { ".json", ".xml", ".pdf" };
            if (string.IsNullOrEmpty(ext) || !permitidos.Contains(ext))
            {
                TempData["UploadMessage"] = "Formato no permitido. Use .json, .xml o .pdf";
                TempData["ShowUploadModal"] = true;
                return Page();
            }

            const long maxBytes = 10 * 1024 * 1024; // 10 MB
            if (Archivo.Length > maxBytes)
            {
                TempData["UploadMessage"] = "Archivo demasiado grande. Máximo 10 MB.";
                TempData["ShowUploadModal"] = true;
                return Page();
            }

            var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            Directory.CreateDirectory(uploads);
            var safeFileName = Path.GetFileName(Archivo.FileName);
            var filePath = Path.Combine(uploads, safeFileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await Archivo.CopyToAsync(stream);
            }

            TempData["UploadMessage"] = "Archivo subido correctamente.";
            TempData["UploadedFile"] = safeFileName;
            TempData["ShowUploadModal"] = true;

            // ✅ Obtener correo del usuario logueado
            string correo = HttpContext.Session.GetString("correo") ?? string.Empty;

            if (string.IsNullOrEmpty(correo))
            {
                TempData["UploadMessage"] = "Error: No se pudo identificar al usuario.";
                TempData["ShowUploadModal"] = true;
                return Page();
            }

            // ✅ Guardar en SQLite
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            // ✅ 1. Obtener el ID del último formulario del usuario
            var getIdCmd = connection.CreateCommand();
            getIdCmd.CommandText = @"
        SELECT Id 
        FROM Formularios
        WHERE Correo = $correo
        ORDER BY Id DESC
        LIMIT 1;
    ";
            getIdCmd.Parameters.AddWithValue("$correo", correo);

            var result = getIdCmd.ExecuteScalar();

            if (result == null)
            {
                TempData["UploadMessage"] = "Error: No se encontró un formulario previo.";
                TempData["ShowUploadModal"] = true;
                return Page();
            }

            int formularioId = Convert.ToInt32(result);

            // ✅ 2. Actualizar el formulario con los datos del archivo
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
        UPDATE Formularios SET
        ArchivoNombre = $nombre
        WHERE Id = $id;
    ";

            updateCmd.Parameters.AddWithValue("$id", formularioId);
            updateCmd.Parameters.AddWithValue("$nombre", safeFileName);
            updateCmd.ExecuteNonQuery();

            return Page();

        }

    }
}



























