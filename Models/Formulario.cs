
namespace form.Models
{
    public class Formulario
    {
        public int Id { get; set; }

        // Campos “clásicos”
        public string Correo { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? RFC { get; set; }
        public string? CURP { get; set; }
        public string? Folio { get; set; }
        public string? Telefono { get; set; }
        public string? Calle { get; set; }
        public string? Numero { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Estado { get; set; }
        public string? Municipio { get; set; }
        public string? RazonSocial { get; set; }
        public string? Fecha { get; set; }

        // Campos nuevos
        public string? Area { get; set; }
        public string? Empresa { get; set; }
        public string? ISO { get; set; }
        public string? NOM { get; set; }
        public string? Contrato { get; set; }
        public string? Solicitud { get; set; }
        public string? Requerimiento { get; set; }
        public string? Permiso { get; set; }
        public string? Peticion { get; set; }

        // Campos de normativa
        public string? Regulacion { get; set; }
        public string? Ley { get; set; }
        public string? Articulo { get; set; }
        public string? Parrafo { get; set; }

        // Archivo
        public string? ArchivoNombre { get; set; }

        // Estado de revisión
        public string? EstadoRevision { get; set; }  // Aprobado | EnRevision | Rechazado
        // NOTA: No existe UpdatedAt en tu schema, así que NO lo incluimos
    }
}
