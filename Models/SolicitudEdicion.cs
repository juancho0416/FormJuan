
using System.Collections.Generic;

namespace form.Models
{
    public class SolicitudEdicionModel
    {
        public int Id { get; set; }
        public int FormularioId { get; set; }
        public string PropuestoPor { get; set; } = "";
        public string? Motivo { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string AntesJson { get; set; } = "";
        public string DespuesJson { get; set; } = "";
        public string CambiosJson { get; set; } = "";
        public string? FechaPropuesta { get; set; }
        public string? RevisadoPor { get; set; }
        public string? FechaRevision { get; set; }
    }

    public class CambioCampo
    {
        public string Antes { get; set; } = "";
        public string Despues { get; set; } = "";
    }
}
