
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System;
using System.Text.Json;

namespace form.Pages.Auditoria
{
    public class DetalleModel : PageModel
    {
        // Parámetro de ruta
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        // Registro cargado
        public AuditoriaRow? Item { get; set; }

        // Fecha convertida a CDMX (solo para mostrar)
        public string? FechaCdmx { get; set; }

        // Detalle pretty (si es JSON)
        public string? DetalleBonito { get; set; }

        // URL de exportación
        public string ExportUrl => Url.Page(null, new { id = Id, handler = "Export" }) ?? $"?id={Id}&handler=Export";

        public void OnGet()
        {
            Item = CargarAuditoriaPorId(Id);

            if (Item != null)
            {
                // Convertir Fecha (string) a DateTime y luego a CDMX para mostrar
                if (DateTime.TryParse(Item.Fecha, out var dt))
                {
                    try
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
                        // La fecha en tu tabla se guarda sin información de zona.
                        // La tratamos como "hora local" y la mostramos en CDMX.
                        var unspecified = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
                        var cdmx = TimeZoneInfo.ConvertTime(unspecified, tz);
                        FechaCdmx = cdmx.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    catch
                    {
                        FechaCdmx = Item.Fecha;
                    }
                }
                else
                {
                    FechaCdmx = Item.Fecha;
                }

                // Intentar pretty-print si Detalle es JSON
                if (!string.IsNullOrWhiteSpace(Item.Detalle))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(Item.Detalle);
                        DetalleBonito = JsonSerializer.Serialize(doc, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                    }
                    catch
                    {
                        DetalleBonito = "(El campo Detalle no es JSON válido)";
                    }
                }
            }
        }

        public IActionResult OnGetExport()
        {
            var item = CargarAuditoriaPorId(Id);
            if (item == null)
                return NotFound();

            var json = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });
            var fileName = $"auditoria_{Id}.json";
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }

        private AuditoriaRow? CargarAuditoriaPorId(int id)
        {
            // Conexión directa sin modificar tu servicio
            using var con = new SqliteConnection("Data Source=usuarios.db");
            con.Open();

            // Leer columnas base
            using var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Usuario, Accion, Entidad, EntidadId, Fecha
                FROM Auditoria
                WHERE Id = $id;
            ";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            var row = new AuditoriaRow
            {
                Id = reader.GetInt32(0),
                Usuario = reader.GetString(1),
                Accion = reader.GetString(2),
                Entidad = reader.GetString(3),
                EntidadId = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                Fecha = reader.IsDBNull(5) ? "" : reader.GetString(5)
            };

            // Intentar leer columna opcional Detalle (si existe)
            try
            {
                using var cmd2 = con.CreateCommand();
                cmd2.CommandText = @"
                    SELECT Detalle
                    FROM Auditoria
                    WHERE Id = $id;
                ";
                cmd2.Parameters.AddWithValue("$id", id);
                var detalle = cmd2.ExecuteScalar();
                if (detalle != null && detalle != DBNull.Value)
                    row.Detalle = detalle.ToString();
            }
            catch
            {
                // La columna no existe; omitir
            }

            return row;
        }

        public class AuditoriaRow
        {
            public int Id { get; set; }
            public string Usuario { get; set; } = "";
            public string Accion { get; set; } = "";
            public string Entidad { get; set; } = "";
            public int? EntidadId { get; set; }
            public string Fecha { get; set; } = "";
            public string? Detalle { get; set; } // opcional
        }
    }
}
