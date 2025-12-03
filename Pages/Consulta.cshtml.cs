using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

// iText7
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Layout.Properties;
using iText.IO.Font.Constants;


namespace form.Pages
{
    public class ConsultaModel : PageModel
    {
        private readonly EmailService _email;

        public ConsultaModel(EmailService email)
        {
            _email = email;
        }

        [BindProperty]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string CorreoBusqueda { get; set; } = string.Empty;

        public List<Dictionary<string, string>> Formularios { get; set; } = new();

        public string Mensaje { get; set; } = string.Empty;

        public string TipoAlerta { get; set; } = "alert-info";

        public void OnGet() { }

        public void OnPost()
        {
            if (string.IsNullOrWhiteSpace(CorreoBusqueda))
            {
                Mensaje = "Ingrese un correo para buscar.";
                TipoAlerta = "alert-danger";
                return;
            }

            Formularios = ObtenerFormularios(CorreoBusqueda);

            if (Formularios.Count > 0)
            {
                Mensaje = "Datos encontrados.";
                TipoAlerta = "alert-success";
            }
            else
            {
                Mensaje = "No se encontró información relacionada a ese correo.";
                TipoAlerta = "alert-danger";
            }
        }

        // Handler para enviar correo con todos los formularios
        public async Task<IActionResult> OnPostEnviarCorreoAsync()
        {
            if (string.IsNullOrWhiteSpace(CorreoBusqueda))
            {
                Mensaje = "Ingrese un correo para enviar.";
                TipoAlerta = "alert-danger";
                return Page();
            }

            var formularios = ObtenerFormularios(CorreoBusqueda);

            if (formularios.Count == 0)
            {
                Mensaje = "No se encontró información relacionada a ese correo.";
                TipoAlerta = "alert-danger";
                return Page();
            }

            var builder = new System.Text.StringBuilder();
            builder.Append("<h2>Todos tus formularios</h2>");

            foreach (var form in formularios)
            {
                builder.Append("<hr/>");
                foreach (var kv in form)
                {
                    builder.Append($"<p><strong>{kv.Key}:</strong> {kv.Value}</p>");
                }
            }

            await _email.EnviarCorreoGenerico(
                CorreoBusqueda,
                "Consulta de formularios",
                builder.ToString()
            );

            Mensaje = "Correo enviado correctamente.";
            TipoAlerta = "alert-success";
            return Page();
        }

        // Handler para exportar a PDF
        public IActionResult OnPostExportarPdf()
        {
            var formularios = ObtenerFormularios(CorreoBusqueda);

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                using var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Título
                doc.Add(new Paragraph("Consulta de Formularios")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER));

                doc.Add(new Paragraph($"Correo: {CorreoBusqueda}")
                    .SetFont(normalFont)
                    .SetFontSize(12));

                doc.Add(new Paragraph("\n"));

                // Recorrer cada formulario
                foreach (var form in formularios)
                {
                    // Crear tabla de 2 columnas
                    Table table = new Table(2).UseAllAvailableWidth();

                    foreach (var kv in form)
                    {
                        // Columna 1: nombre del campo en negritas
                        table.AddCell(new Cell().Add(new Paragraph(kv.Key).SetFont(boldFont)));

                        // Columna 2: valor del campo
                        table.AddCell(new Cell().Add(new Paragraph(kv.Value).SetFont(normalFont)));
                    }

                    // Agregar tabla al documento
                    doc.Add(table);

                    // Separador entre formularios
                    doc.Add(new Paragraph("\n-----------------------------\n"));
                }
            }

            return File(ms.ToArray(), "application/pdf", "consulta.pdf");
        }

        // Método reutilizable para obtener formularios
        private List<Dictionary<string, string>> ObtenerFormularios(string correo)
        {
            var lista = new List<Dictionary<string, string>>();

            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT *
                FROM Formularios
                WHERE Correo = $correo
                ORDER BY Id DESC;
            ";
            command.Parameters.AddWithValue("$correo", correo.Trim().ToLower());

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var datos = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columna = reader.GetName(i);
                    string valor = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();
                    datos[columna] = valor;
                }
                lista.Add(datos);
            }

            return lista;
        }
    }
}
