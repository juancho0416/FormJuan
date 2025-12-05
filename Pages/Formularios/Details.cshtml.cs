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
    public class DetailsModel : PageModel
    {

        public FormularioRecord Formulario { get; set; } = new();

        public void OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id, Correo, Nombre, RFC, CURP, Folio, Telefono, Calle, Numero,
                                           CodigoPostal, Estado, Municipio, RazonSocial, Fecha, Area,
                                            Empresa, ISO, NOM, Contrato, Requerimiento, Permiso, Peticion,
                                            Regulacion, Ley, Articulo, Parrafo
                                    FROM Formularios
                                    WHERE Id = $id";
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
                    Fecha = reader.GetString(13),
                    Area = reader.GetString(14),
                    Empresa = reader.GetString(15),
                    ISO = reader.GetString(16),
                    NOM = reader.GetString(17),
                    Contrato = reader.GetString(18),
                    Requerimiento = reader.GetString(19),
                    Permiso = reader.GetString(20),
                    Peticion = reader.GetString(21),
                    Regulacion = reader.GetString(22),
                    Ley = reader.GetString(23),
                    Articulo = reader.GetString(24),
                    Parrafo = reader.GetString(25)
                };
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
            public string Area { get; set; }
            public string Empresa { get; set; }
            public string ISO { get; set; }
            public string NOM { get; set; }
            public string Contrato { get; set; }
            public string Requerimiento { get; set; }
            public string Permiso { get; set; }
            public string Peticion { get; set; }
            public string Regulacion { get; set; }
            public string Ley { get; set; }
            public string Articulo { get; set; }
            public string Parrafo { get; set; }
        }


        private Dictionary<string, string> ObtenerFormularioPorId(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"SELECT * FROM Formularios WHERE Id = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var datos = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columna = reader.GetName(i);
                    string valor = reader.IsDBNull(i) ? "" : reader.GetValue(i).ToString();
                    datos[columna] = valor;
                }
                return datos;
            }

            return new Dictionary<string, string>();
        }

        public IActionResult OnPostExportarPdf(int id)
        {
            var form = ObtenerFormularioPorId(id);

            if (form.Count == 0)
            {
                return NotFound("No se encontró el formulario.");
            }

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                using var pdf = new PdfDocument(writer);
                var doc = new Document(pdf);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Título
                doc.Add(new Paragraph("Detalle del Formulario")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.CENTER));

                doc.Add(new Paragraph($"ID: {id}")
                    .SetFont(normalFont)
                    .SetFontSize(12));

                doc.Add(new Paragraph("\n"));

                // Crear tabla de 2 columnas
                Table table = new Table(2).UseAllAvailableWidth();

                foreach (var kv in form)
                {
                    table.AddCell(new Cell().Add(new Paragraph(kv.Key).SetFont(boldFont)));
                    table.AddCell(new Cell().Add(new Paragraph(kv.Value).SetFont(normalFont)));
                }

                doc.Add(table);
                doc.Close();
            }

            return File(ms.ToArray(), "application/pdf", $"formulario_{id}.pdf");
        }

    }
}