namespace form.Models
{
    public class Auditoria
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Entidad { get; set; } = string.Empty;
        public int EntidadId { get; set; }
        public string Fecha { get; set; } = string.Empty;
    }
}
