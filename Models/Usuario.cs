namespace form.Models
{
    public class UsuarioRecord
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Rol { get; set; } = "";
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
    }
}
