
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using form.Models;
using form.Services;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace form.Pages.Formularios
{
    public class EditarModel : PageModel
    {
        [BindProperty] public Formulario Formulario { get; set; } = new();
        [BindProperty] public string? MotivoPropuesta { get; set; }  // campo para escribir el motivo

        private readonly AuditoriaService _auditoria;
        public EditarModel(AuditoriaService auditoria) => _auditoria = auditoria;

        public IActionResult OnGet(int id)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();
            EnableForeignKeys(connection);

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Correo, Nombre, RFC, CURP, Folio, Telefono, Calle, Numero, CodigoPostal,
                       Estado, Municipio, RazonSocial, Fecha,
                       Area, Empresa, ISO, NOM, Contrato, Solicitud, Requerimiento, Permiso, Peticion,
                       Regulacion, Ley, Articulo, Parrafo, ArchivoNombre, EstadoRevision
                FROM Formularios
                WHERE Id = $id;
            ";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                TempData["Mensaje"] = "Formulario no encontrado.";
                return RedirectToPage("/Formularios/Index");
            }

            Formulario = new Formulario
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Correo = reader["Correo"]?.ToString() ?? "",
                Nombre = reader["Nombre"]?.ToString(),
                RFC = reader["RFC"]?.ToString(),
                CURP = reader["CURP"]?.ToString(),
                Folio = reader["Folio"]?.ToString(),
                Telefono = reader["Telefono"]?.ToString(),
                Calle = reader["Calle"]?.ToString(),
                Numero = reader["Numero"]?.ToString(),
                CodigoPostal = reader["CodigoPostal"]?.ToString(),
                Estado = reader["Estado"]?.ToString(),
                Municipio = reader["Municipio"]?.ToString(),
                RazonSocial = reader["RazonSocial"]?.ToString(),
                Fecha = reader["Fecha"]?.ToString(),

                Area = reader["Area"]?.ToString(),
                Empresa = reader["Empresa"]?.ToString(),
                ISO = reader["ISO"]?.ToString(),
                NOM = reader["NOM"]?.ToString(),
                Contrato = reader["Contrato"]?.ToString(),
                Solicitud = reader["Solicitud"]?.ToString(),
                Requerimiento = reader["Requerimiento"]?.ToString(),
                Permiso = reader["Permiso"]?.ToString(),
                Peticion = reader["Peticion"]?.ToString(),

                Regulacion = reader["Regulacion"]?.ToString(),
                Ley = reader["Ley"]?.ToString(),
                Articulo = reader["Articulo"]?.ToString(),
                Parrafo = reader["Parrafo"]?.ToString(),

                ArchivoNombre = reader["ArchivoNombre"]?.ToString(),
                EstadoRevision = reader["EstadoRevision"]?.ToString()
            };

            return Page();
        }

        public IActionResult OnPost()
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();
            EnableForeignKeys(connection);

            // 1) Leer "antes" del DB
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = @"
                SELECT Id, Correo, Nombre, RFC, CURP, Folio, Telefono, Calle, Numero, CodigoPostal,
                       Estado, Municipio, RazonSocial, Fecha,
                       Area, Empresa, ISO, NOM, Contrato, Solicitud, Requerimiento, Permiso, Peticion,
                       Regulacion, Ley, Articulo, Parrafo, ArchivoNombre
                FROM Formularios
                WHERE Id = $id;
            ";
            selectCmd.Parameters.AddWithValue("$id", Formulario.Id);

            Formulario? antes = null;
            using (var reader = selectCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    antes = new Formulario
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Correo = reader["Correo"]?.ToString() ?? "",
                        Nombre = reader["Nombre"]?.ToString(),
                        RFC = reader["RFC"]?.ToString(),
                        CURP = reader["CURP"]?.ToString(),
                        Folio = reader["Folio"]?.ToString(),
                        Telefono = reader["Telefono"]?.ToString(),
                        Calle = reader["Calle"]?.ToString(),
                        Numero = reader["Numero"]?.ToString(),
                        CodigoPostal = reader["CodigoPostal"]?.ToString(),
                        Estado = reader["Estado"]?.ToString(),
                        Municipio = reader["Municipio"]?.ToString(),
                        RazonSocial = reader["RazonSocial"]?.ToString(),
                        Fecha = reader["Fecha"]?.ToString(),

                        Area = reader["Area"]?.ToString(),
                        Empresa = reader["Empresa"]?.ToString(),
                        ISO = reader["ISO"]?.ToString(),
                        NOM = reader["NOM"]?.ToString(),
                        Contrato = reader["Contrato"]?.ToString(),
                        Solicitud = reader["Solicitud"]?.ToString(),
                        Requerimiento = reader["Requerimiento"]?.ToString(),
                        Permiso = reader["Permiso"]?.ToString(),
                        Peticion = reader["Peticion"]?.ToString(),

                        Regulacion = reader["Regulacion"]?.ToString(),
                        Ley = reader["Ley"]?.ToString(),
                        Articulo = reader["Articulo"]?.ToString(),
                        Parrafo = reader["Parrafo"]?.ToString(),

                        ArchivoNombre = reader["ArchivoNombre"]?.ToString()
                    };
                }
            }

            if (antes == null)
            {
                TempData["Mensaje"] = "No se encontró el formulario.";
                return RedirectToPage("/Formularios/Index");
            }

            // 2) Construir "después" con lo posteado
            var d = Formulario;
            var despues = new Formulario
            {
                Id = d.Id,
                Correo = d.Correo ?? "",
                Nombre = d.Nombre ?? "",
                RFC = d.RFC ?? "",
                CURP = d.CURP ?? "",
                Folio = d.Folio ?? "",
                Telefono = d.Telefono ?? "",
                Calle = d.Calle ?? "",
                Numero = d.Numero ?? "",
                CodigoPostal = d.CodigoPostal ?? "",
                Estado = d.Estado ?? "",
                Municipio = d.Municipio ?? "",
                RazonSocial = d.RazonSocial ?? "",
                Fecha = d.Fecha ?? "",

                Area = d.Area ?? "",
                Empresa = d.Empresa ?? "",
                ISO = d.ISO ?? "",
                NOM = d.NOM ?? "",
                Contrato = d.Contrato ?? "",
                Solicitud = d.Solicitud ?? "",
                Requerimiento = d.Requerimiento ?? "",
                Permiso = d.Permiso ?? "",
                Peticion = d.Peticion ?? "",

                Regulacion = d.Regulacion ?? "",
                Ley = d.Ley ?? "",
                Articulo = d.Articulo ?? "",
                Parrafo = d.Parrafo ?? "",

                ArchivoNombre = d.ArchivoNombre ?? ""
            };

            // 3) Detectar cambios
            var cambios = CompararCambios(antes, despues);
            if (cambios.Count == 0)
            {
                TempData["Mensaje"] = "No hay cambios para enviar a revisión.";
                return RedirectToPage("/Formularios/Index");
            }

            var usuario = HttpContext.Session.GetString("correo") ?? "anonimo";
            var fechaStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // 4) Insertar solicitud en SolicitudesEdicion (tu tabla existe)
            var insertSolicitud = connection.CreateCommand();
            insertSolicitud.CommandText = @"
                INSERT INTO SolicitudesEdicion (FormularioId, PropuestoPor, Motivo, Estado, AntesJson, DespuesJson, CambiosJson, FechaPropuesta)
                VALUES ($fid, $user, $motivo, 'Pendiente', $antes, $despues, $cambios, $fecha);
            ";
            insertSolicitud.Parameters.AddWithValue("$fid", Formulario.Id);
            insertSolicitud.Parameters.AddWithValue("$user", usuario);
            insertSolicitud.Parameters.AddWithValue("$motivo", string.IsNullOrWhiteSpace(MotivoPropuesta) ? "Actualización de datos" : MotivoPropuesta);
            insertSolicitud.Parameters.AddWithValue("$antes", JsonSerializer.Serialize(antes));
            insertSolicitud.Parameters.AddWithValue("$despues", JsonSerializer.Serialize(despues));
            insertSolicitud.Parameters.AddWithValue("$cambios", JsonSerializer.Serialize(cambios));
            insertSolicitud.Parameters.AddWithValue("$fecha", fechaStr);
            insertSolicitud.ExecuteNonQuery();

            // Obtener id autoincrement de la solicitud (para notificación)
            int solicitudId = 0;
            using (var idCmd = connection.CreateCommand())
            {
                idCmd.CommandText = "SELECT last_insert_rowid();";
                solicitudId = Convert.ToInt32(idCmd.ExecuteScalar());
            }

            // 5) Marcar formulario como EnRevision
            var setRevision = connection.CreateCommand();
            setRevision.CommandText = @"UPDATE Formularios SET EstadoRevision = 'EnRevision' WHERE Id = $fid;";
            setRevision.Parameters.AddWithValue("$fid", Formulario.Id);
            setRevision.ExecuteNonQuery();

            // (Opcional) Notificar a Admin/Revisor
            try
            {
                var notifSvc = HttpContext.RequestServices.GetRequiredService<NotificacionService>();
                var urlPanel = Url.Page("/Formularios/Solicitudes/Index") ?? "/Formularios/Solicitudes";
                notifSvc.CrearParaRol(
                    rol: "Admin",
                    titulo: $"Nueva solicitud #{solicitudId} del formulario {Formulario.Id}",
                    mensaje: $"El usuario {usuario} envió cambios para revisión.",
                    tipo: "PROPUESTA",
                    link: urlPanel
                );
                // Si usas rol Revisor:
                notifSvc.CrearParaRol(
                    rol: "Revisor",
                    titulo: $"Nueva solicitud #{solicitudId} del formulario {Formulario.Id}",
                    mensaje: $"El usuario {usuario} envió cambios para revisión.",
                    tipo: "PROPUESTA",
                    link: urlPanel
                );
            }
            catch { /* no bloquear si falla notificación */ }

            // 6) Auditoría
            var detalle = new
            {
                Contexto = new
                {
                    Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UsuarioSesion = usuario,
                    FormularioId = Formulario.Id,
                    FechaLocal = fechaStr,
                    Navegador = Request.Headers["User-Agent"].ToString()
                },
                CambiosPropuestos = cambios
            };
            var detalleJson = JsonSerializer.Serialize(detalle);

            try
            {
                _auditoria.Registrar(
                    usuario: usuario,
                    accion: "PROPUESTA_EDICION",
                    entidad: "Formulario",
                    entidadId: Formulario.Id,
                    fecha: fechaStr,
                    detalle: detalleJson
                );
            }
            catch { }

            TempData["Mensaje"] = "Solicitud de edición enviada para aprobación.";
            // Si prefieres llevar al panel directamente:
            // return RedirectToPage("/Formularios/Solicitudes/Index");
            return RedirectToPage("/Formularios/Index");
        }

        // ---- Helpers ----

        private Dictionary<string, CambioCampo> CompararCambios(Formulario a, Formulario d)
        {
            var cambios = new Dictionary<string, CambioCampo>(StringComparer.OrdinalIgnoreCase);
            void Add(string campo, string av, string dv)
            {
                av = av ?? ""; dv = dv ?? "";
                if (!string.Equals(av, dv, StringComparison.Ordinal))
                    cambios[campo] = new CambioCampo { Antes = av, Despues = dv };
            }

            // Clásicos
            Add("Correo", a.Correo, d.Correo);
            Add("Nombre", a.Nombre ?? "", d.Nombre ?? "");
            Add("RFC", a.RFC ?? "", d.RFC ?? "");
            Add("CURP", a.CURP ?? "", d.CURP ?? "");
            Add("Folio", a.Folio ?? "", d.Folio ?? "");
            Add("Telefono", a.Telefono ?? "", d.Telefono ?? "");
            Add("Calle", a.Calle ?? "", d.Calle ?? "");
            Add("Numero", a.Numero ?? "", d.Numero ?? "");
            Add("CodigoPostal", a.CodigoPostal ?? "", d.CodigoPostal ?? "");
            Add("Estado", a.Estado ?? "", d.Estado ?? "");
            Add("Municipio", a.Municipio ?? "", d.Municipio ?? "");
            Add("RazonSocial", a.RazonSocial ?? "", d.RazonSocial ?? "");
            Add("Fecha", a.Fecha ?? "", d.Fecha ?? "");

            // Nuevos
            Add("Area", a.Area ?? "", d.Area ?? "");
            Add("Empresa", a.Empresa ?? "", d.Empresa ?? "");
            Add("ISO", a.ISO ?? "", d.ISO ?? "");
            Add("NOM", a.NOM ?? "", d.NOM ?? "");
            Add("Contrato", a.Contrato ?? "", d.Contrato ?? "");
            Add("Solicitud", a.Solicitud ?? "", d.Solicitud ?? "");
            Add("Requerimiento", a.Requerimiento ?? "", d.Requerimiento ?? "");
            Add("Permiso", a.Permiso ?? "", d.Permiso ?? "");
            Add("Peticion", a.Peticion ?? "", d.Peticion ?? "");

            // Normativa
            Add("Regulacion", a.Regulacion ?? "", d.Regulacion ?? "");
            Add("Ley", a.Ley ?? "", d.Ley ?? "");
            Add("Articulo", a.Articulo ?? "", d.Articulo ?? "");
            Add("Parrafo", a.Parrafo ?? "", d.Parrafo ?? "");

            // Archivo
            Add("ArchivoNombre", a.ArchivoNombre ?? "", d.ArchivoNombre ?? "");

            return cambios;
        }

        private void EnableForeignKeys(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }
    }

    // Modelo para el diff
    public class CambioCampo
    {
        public string Antes { get; set; } = "";
        public string Despues { get; set; } = "";
    }
}
