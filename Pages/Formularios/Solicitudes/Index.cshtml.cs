
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Text.Json;
using form.Models;
using form.Services;
using System;

namespace form.Pages.Formularios.Solicitudes
{
    public class IndexModel : PageModel
    {
        public List<SolicitudItemVM> Pendientes { get; set; } = new();

        private readonly AuditoriaService _auditoria;
        public IndexModel(AuditoriaService auditoria) => _auditoria = auditoria;

        public class SolicitudItemVM
        {
            public int Id { get; set; }
            public int FormularioId { get; set; }
            public string PropuestoPor { get; set; } = "";
            public string? Motivo { get; set; }
            public string? FechaPropuesta { get; set; }
            public Dictionary<string, CambioCampo> Cambios { get; set; } = new();
        }

        public IActionResult OnGet()
        {
            using var conn = new SqliteConnection("Data Source=usuarios.db");
            conn.Open();
            EnableForeignKeys(conn);

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, FormularioId, PropuestoPor, Motivo, CambiosJson, FechaPropuesta
                FROM SolicitudesEdicion
                WHERE Estado = 'Pendiente'
                ORDER BY FechaPropuesta DESC;
            ";

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var item = new SolicitudItemVM
                {
                    Id = r.GetInt32(0),
                    FormularioId = r.GetInt32(1),
                    PropuestoPor = r["PropuestoPor"]?.ToString() ?? "",
                    Motivo = r["Motivo"]?.ToString(),
                    FechaPropuesta = r["FechaPropuesta"]?.ToString()
                };

                var cambiosJson = r["CambiosJson"]?.ToString() ?? "{}";
                item.Cambios = JsonSerializer.Deserialize<Dictionary<string, CambioCampo>>(cambiosJson)
                               ?? new Dictionary<string, CambioCampo>(StringComparer.OrdinalIgnoreCase);

                Pendientes.Add(item);
            }

            return Page();
        }

        public IActionResult OnPostAprobar(int solicitudId)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();
            EnableForeignKeys(connection);

            // Cargar solicitud pendiente
            var getSol = connection.CreateCommand();
            getSol.CommandText = @"
                SELECT FormularioId, DespuesJson
                FROM SolicitudesEdicion
                WHERE Id = $id AND Estado = 'Pendiente';
            ";
            getSol.Parameters.AddWithValue("$id", solicitudId);

            int formularioId = 0;
            Formulario? despues = null;

            using (var r = getSol.ExecuteReader())
            {
                if (!r.Read())
                {
                    TempData["Mensaje"] = "Solicitud no encontrada o ya revisada.";
                    return RedirectToPage();
                }
                formularioId = r.GetInt32(0);
                var jsonDespues = r.GetString(1);
                despues = JsonSerializer.Deserialize<Formulario>(jsonDespues);
            }

            // Aplicar UPDATE al formulario
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Formularios
                SET Correo = $correo,
                    Nombre = $nombre,
                    RFC = $rfc,
                    CURP = $curp,
                    Folio = $folio,
                    Telefono = $telefono,
                    Calle = $calle,
                    Numero = $numero,
                    CodigoPostal = $cp,
                    Estado = $estado,
                    Municipio = $municipio,
                    RazonSocial = $razon,
                    Fecha = $fecha,
                    Area = $area,
                    Empresa = $empresa,
                    ISO = $iso,
                    NOM = $nom,
                    Contrato = $contrato,
                    Solicitud = $solicitud,
                    Requerimiento = $requerimiento,
                    Permiso = $permiso,
                    Peticion = $peticion,
                    Regulacion = $reg,
                    Ley = $ley,
                    Articulo = $art,
                    Parrafo = $parrafo,
                    ArchivoNombre = $archivo,
                    EstadoRevision = 'Aprobado'
                WHERE Id = $id;
            ";
            updateCmd.Parameters.AddWithValue("$id", formularioId);

            updateCmd.Parameters.AddWithValue("$correo", despues?.Correo ?? "");
            updateCmd.Parameters.AddWithValue("$nombre", despues?.Nombre ?? "");
            updateCmd.Parameters.AddWithValue("$rfc", despues?.RFC ?? "");
            updateCmd.Parameters.AddWithValue("$curp", despues?.CURP ?? "");
            updateCmd.Parameters.AddWithValue("$folio", despues?.Folio ?? "");
            updateCmd.Parameters.AddWithValue("$telefono", despues?.Telefono ?? "");
            updateCmd.Parameters.AddWithValue("$calle", despues?.Calle ?? "");
            updateCmd.Parameters.AddWithValue("$numero", despues?.Numero ?? "");
            updateCmd.Parameters.AddWithValue("$cp", despues?.CodigoPostal ?? "");
            updateCmd.Parameters.AddWithValue("$estado", despues?.Estado ?? "");
            updateCmd.Parameters.AddWithValue("$municipio", despues?.Municipio ?? "");
            updateCmd.Parameters.AddWithValue("$razon", despues?.RazonSocial ?? "");
            updateCmd.Parameters.AddWithValue("$fecha", despues?.Fecha ?? "");

            updateCmd.Parameters.AddWithValue("$area", despues?.Area ?? "");
            updateCmd.Parameters.AddWithValue("$empresa", despues?.Empresa ?? "");
            updateCmd.Parameters.AddWithValue("$iso", despues?.ISO ?? "");
            updateCmd.Parameters.AddWithValue("$nom", despues?.NOM ?? "");
            updateCmd.Parameters.AddWithValue("$contrato", despues?.Contrato ?? "");
            updateCmd.Parameters.AddWithValue("$solicitud", despues?.Solicitud ?? "");
            updateCmd.Parameters.AddWithValue("$requerimiento", despues?.Requerimiento ?? "");
            updateCmd.Parameters.AddWithValue("$permiso", despues?.Permiso ?? "");
            updateCmd.Parameters.AddWithValue("$peticion", despues?.Peticion ?? "");

            updateCmd.Parameters.AddWithValue("$reg", despues?.Regulacion ?? "");
            updateCmd.Parameters.AddWithValue("$ley", despues?.Ley ?? "");
            updateCmd.Parameters.AddWithValue("$art", despues?.Articulo ?? "");
            updateCmd.Parameters.AddWithValue("$parrafo", despues?.Parrafo ?? "");
            updateCmd.Parameters.AddWithValue("$archivo", despues?.ArchivoNombre ?? "");

            updateCmd.ExecuteNonQuery();

            // Cerrar solicitud como Aprobada
            var usuarioAprobador = HttpContext.Session.GetString("correo") ?? "anonimo";
            var fechaStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var closeSol = connection.CreateCommand();
            closeSol.CommandText = @"
                UPDATE SolicitudesEdicion
                SET Estado = 'Aprobado',
                    RevisadoPor = $user,
                    FechaRevision = $fecha
                WHERE Id = $id;
            ";
            closeSol.Parameters.AddWithValue("$user", usuarioAprobador);
            closeSol.Parameters.AddWithValue("$fecha", fechaStr);
            closeSol.Parameters.AddWithValue("$id", solicitudId);
            closeSol.ExecuteNonQuery();
            string propuestoPor = "";
            using (var getUser = connection.CreateCommand())
            {
                getUser.CommandText = "SELECT PropuestoPor FROM SolicitudesEdicion WHERE Id = $id;";
                getUser.Parameters.AddWithValue("$id", solicitudId);
                propuestoPor = (getUser.ExecuteScalar()?.ToString() ?? "");
            }

            try
            {
                var notifSvc = HttpContext.RequestServices.GetRequiredService<NotificacionService>();
                var linkForm = Url.Page("/Formularios/Editar", new { id = formularioId }) ?? $"/Formularios/Editar/{formularioId}";
                notifSvc.CrearParaUsuario(
                    correo: propuestoPor,
                    titulo: $"Solicitud #{solicitudId} aprobada",
                    mensaje: $"Tus cambios del formulario {formularioId} fueron aprobados.",
                    tipo: "APROBACION",
                    link: linkForm
                );
            }
            catch { }
            // Auditoría
            try
            {
                _auditoria.Registrar(
                    usuario: usuarioAprobador,
                    accion: "APROBAR_EDICION",
                    entidad: "Formulario",
                    entidadId: formularioId,
                    fecha: fechaStr,
                    detalle: JsonSerializer.Serialize(new { SolicitudId = solicitudId })
                );
            }
            catch { }

            TempData["Mensaje"] = "Cambios aprobados y aplicados.";
            return RedirectToPage();
        }

        public IActionResult OnPostRechazar(int solicitudId, string? motivoRechazo)
        {
            using var connection = new SqliteConnection("Data Source=usuarios.db");
            connection.Open();
            EnableForeignKeys(connection);

            var usuarioAprobador = HttpContext.Session.GetString("correo") ?? "anonimo";
            var fechaStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Rechazar solicitud
            var rej = connection.CreateCommand();
            rej.CommandText = @"
                UPDATE SolicitudesEdicion
                SET Estado = 'Rechazado',
                    RevisadoPor = $user,
                    FechaRevision = $fecha,
                    Motivo = COALESCE(Motivo, '') || CASE WHEN $motivo <> '' THEN (' | Rechazado: ' || $motivo) ELSE '' END
                WHERE Id = $id AND Estado = 'Pendiente';
            ";
            rej.Parameters.AddWithValue("$user", usuarioAprobador);
            rej.Parameters.AddWithValue("$fecha", fechaStr);
            rej.Parameters.AddWithValue("$motivo", motivoRechazo ?? "");
            rej.Parameters.AddWithValue("$id", solicitudId);
            rej.ExecuteNonQuery();

            // Marcar formulario como Rechazado
            var getFid = connection.CreateCommand();
            getFid.CommandText = @"SELECT FormularioId FROM SolicitudesEdicion WHERE Id = $id;";
            getFid.Parameters.AddWithValue("$id", solicitudId);
            int formularioId = Convert.ToInt32(getFid.ExecuteScalar());

            var markForm = connection.CreateCommand();
            markForm.CommandText = @"UPDATE Formularios SET EstadoRevision = 'Rechazado' WHERE Id = $fid;";
            markForm.Parameters.AddWithValue("$fid", formularioId);
            markForm.ExecuteNonQuery();




            string propuestoPor = "";
            using (var getUser = connection.CreateCommand())
            {
                getUser.CommandText = "SELECT PropuestoPor FROM SolicitudesEdicion WHERE Id = $id;";
                getUser.Parameters.AddWithValue("$id", solicitudId);
                propuestoPor = (getUser.ExecuteScalar()?.ToString() ?? "");
            }

            try
            {
                var notifSvc = HttpContext.RequestServices.GetRequiredService<NotificacionService>();
                var linkForm = Url.Page("/Formularios/Editar", new { id = formularioId }) ?? $"/Formularios/Editar/{formularioId}";
                notifSvc.CrearParaUsuario(
                    correo: propuestoPor,
                    titulo: $"Solicitud #{solicitudId} rechazada",
                    mensaje: $"Tu solicitud fue rechazada. Motivo: {motivoRechazo ?? "Sin motivo"}",
                    tipo: "RECHAZO",
                    link: linkForm
                );
            }
            catch { }

            // Auditoría
            try
            {
                _auditoria.Registrar(
                    usuario: usuarioAprobador,
                    accion: "RECHAZAR_EDICION",
                    entidad: "Formulario",
                    entidadId: formularioId,
                    fecha: fechaStr,
                    detalle: JsonSerializer.Serialize(new { SolicitudId = solicitudId, Motivo = motivoRechazo ?? "" })
                );
            }
            catch { }

            TempData["Mensaje"] = "La solicitud fue rechazada.";
            return RedirectToPage();
        }

        private void EnableForeignKeys(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }
    }
}
