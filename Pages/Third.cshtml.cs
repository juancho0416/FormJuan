using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using form.Services;
using form.Models;

namespace form.Pages;

public class ThirdModel : PageModel
{
    [BindProperty]
    public FormularioRecord Formulario { get; set; } = new();
    private readonly EmailService _emailService;
    private readonly IWebHostEnvironment _env;

    // Constructor con inyección de dependencias
    public ThirdModel(IWebHostEnvironment env, EmailService emailService)
    {
        _env = env;
        _emailService = emailService;
    }
    // propiedades que usa la vista
    //Seleccion de fecha
    public string? Fecha { get; set; }

    //Primera pagina formulario (Second)
    public string? Nombre { get; set; }
    public string? RFC { get; set; }
    public string? CURP { get; set; }
    public string? Folio { get; set; }
    public string? Telefono { get; set; }
    //segunda parte formulario (TwoFive)
    public string? Area { get; set; }
    public string? Empresa { get; set; }
    public string? ISO { get; set; }
    public string? NOM { get; set; }
    public string? Contrato { get; set; }
    public string? Requerimiento { get; set; }
    public string? Permiso { get; set; }
    public string? Peticion { get; set; }
    //Tercera parte formulario(TwoNineFive)

    public string? Regulacion { get; set; }
    public string? Ley { get; set; }
    public string? Articulo { get; set; }
    public string? Parrafo { get; set; }


    [TempData] public string CodigoPostal { get; set; } = string.Empty;
    [TempData] public string Estado { get; set; } = string.Empty;
    [TempData] public string Municipio { get; set; } = string.Empty;



    //parte de subir archivos validos
    public string? Archivo { get; set; }
    public bool FileExists { get; set; }
    public string? UploadMessage { get; set; }

    // único OnGet: acepta parámetros opcionales (vienen por query string) o usa TempData
    public void OnGet(string? uploadedFile, string? message)
    {


        ///temp data guarda la informacion de los formualrios anteriores en esta pagina 
        /// con tempdata 

        /// /// Primera parte del formulario
        Nombre = TempData["Nombre"]?.ToString();
        RFC = TempData["RFC"]?.ToString();
        CURP = TempData["CURP"]?.ToString();
        Telefono = TempData["Telefono"]?.ToString();
        Folio = TempData["Folio"]?.ToString();

        ///Parte dos del formulario desplegables
        Area = TempData["Area"]?.ToString();
        Empresa = TempData["Empresa"]?.ToString();
        ISO = TempData["ISO"]?.ToString();
        NOM = TempData["NOM"]?.ToString();
        Contrato = TempData["Contrato"]?.ToString();
        Requerimiento = TempData["Requerimiento"]?.ToString();
        Permiso = TempData["Permiso"]?.ToString();
        Peticion = TempData["Peticion"]?.ToString();
        //tercera parte formulario 
        Regulacion = TempData["Regulacion"]?.ToString();
        Ley = TempData["Ley"]?.ToString();
        Articulo = TempData["Articulo"]?.ToString();
        Parrafo = TempData["Parrafo"]?.ToString();

        Fecha = TempData["Fecha"]?.ToString();
        // Prioriza el querystring, si no existe usa TempData (por si viniste desde TwoNine)
        Archivo = !string.IsNullOrWhiteSpace(uploadedFile) ? uploadedFile : TempData["UploadedFile"] as string;
        UploadMessage = !string.IsNullOrWhiteSpace(message) ? message : TempData["UploadMessage"] as string;

        if (!string.IsNullOrWhiteSpace(Archivo))
        {
            var safeName = Path.GetFileName(Archivo);
            var uploads = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            var fullPath = Path.Combine(uploads, safeName);

            FileExists = System.IO.File.Exists(fullPath);
            Archivo = safeName;
            if (!FileExists)
            {
                UploadMessage = "No se encontró el archivo subido.";
            }
            else
            {
                UploadMessage ??= "Archivo encontrado y listo para validar.";
            }
        }
        else
        {
            FileExists = false;
            UploadMessage ??= "No se recibió nombre de archivo.";
        }

        // deja valores en TempData si otras vistas los necesitan
        TempData["UploadedFile"] = Archivo;
        TempData["UploadMessage"] = UploadMessage;
    }

    // onpost para mandar notificacion de formulario completado con exito
    public async Task<IActionResult> OnPostConfirm()
    {
        string correo = HttpContext.Session.GetString("correo") ?? string.Empty;
        if (string.IsNullOrEmpty(correo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo identificar el usuario. Inicia sesión nuevamente.");
            return Page();
        }

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        // Traer el formulario más reciente del usuario
        var getDataCmd = connection.CreateCommand();
        getDataCmd.CommandText = @"
        SELECT Id, Nombre, Empresa, RFC, Fecha
        FROM Formularios
        WHERE Correo = $correo
        ORDER BY Id DESC
        LIMIT 1;
    ";
        getDataCmd.Parameters.AddWithValue("$correo", correo);

        using var reader = getDataCmd.ExecuteReader();
        if (!reader.Read())
        {
            ModelState.AddModelError(string.Empty, "No se encontró un formulario para este usuario.");
            return Page();
        }

        int formularioId = reader.GetInt32(0);
        string nombre = reader.GetString(1);
        string empresa = reader.GetString(2);
        string rfc = reader.GetString(3);
        string fecha = reader.GetString(4);

        //cerrar reader antes de auditoria
        reader.Close();
        //cerrar conexion antess de auditoria
        connection.Close();

        //Auditoria
        var auditoria = new AuditoriaService();
        var usuario = HttpContext.Session.GetString("correo") ?? "anonimo";
        auditoria.Registrar(usuario, "Crear", "Formulario", formularioId);

        //  Enviar correo
        string asunto = "Formulario enviado correctamente";
        string htmlContenido = $@"
        <h2>Formulario enviado</h2>
        <p>Hola {nombre},</p>
        <p>Tu formulario se ha recibido con éxito el día {fecha}.</p>
        <p>Empresa: {empresa}</p>
        <p>RFC: {rfc}</p>
    ";

        //si comento el await _emailService no hay error y si lo deejo hay 
        //error aunque funciona bien pero se traba al dar en confirmar y
        //  solo funciona con varios clicks 

        // await _emailService.EnviarCorreoGenerico(correo, asunto, htmlContenido);

        return RedirectToPage("/Fourth");
    }

    public IActionResult OnPost()
    {

        //primera parte del formulario
        Nombre = TempData["Nombre"]?.ToString();
        RFC = TempData["RFC"]?.ToString();
        CURP = TempData["CURP"]?.ToString();
        Telefono = TempData["Telefono"]?.ToString();

        Folio = TempData["Folio"]?.ToString();
        //segunda parte del formulario
        Area = TempData["Area"]?.ToString();
        Empresa = TempData["Empresa"]?.ToString();
        ISO = TempData["ISO"]?.ToString();
        NOM = TempData["NOM"]?.ToString();
        Contrato = TempData["Contrato"]?.ToString();
        Requerimiento = TempData["Requerimiento"]?.ToString();
        Permiso = TempData["Permiso"]?.ToString();
        Peticion = TempData["Peticion"]?.ToString();
        //tercera parte del formulario
        Regulacion = TempData["Regulacion"]?.ToString();
        Ley = TempData["Ley"]?.ToString();
        Articulo = TempData["Articulo"]?.ToString();
        Parrafo = TempData["Parrafo"]?.ToString();




        return RedirectToPage("Fourth");

    }
}