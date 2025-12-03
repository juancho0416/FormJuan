using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace form.Pages;

public class TwoFiveModel : PageModel
{


    [BindProperty]
    public FormInput Input { get; set; } = new FormInput();



    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Guardar datos en TempData
        TempData["Area"] = Input.Area;
        TempData["Empresa"] = Input.Empresa;
        TempData["ISO"] = Input.ISO;
        TempData["NOM"] = Input.NOM;
        TempData["Contrato"] = Input.Contrato;
        TempData["Requerimiento"] = Input.Requerimiento;
        TempData["Permiso"] = Input.Permiso;

        //  Obtener correo del usuario logueado
        string correo = HttpContext.Session.GetString("correo") ?? string.Empty;

        if (string.IsNullOrEmpty(correo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario. Inicia sesión nuevamente.");
            return Page();
        }

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        //
        // 1. Obtener el ID del último formulario del usuario
        //
        var getIdCmd = connection.CreateCommand();
        getIdCmd.CommandText = @"
    SELECT Id 
    FROM Formularios
    WHERE Correo = $correo
    ORDER BY Id DESC
    LIMIT 1;
";
        getIdCmd.Parameters.AddWithValue("$correo", correo);

        var result = getIdCmd.ExecuteScalar();

        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "No se encontró un formulario previo para este usuario.");
            return Page();
        }

        int formularioId = Convert.ToInt32(result);

        //
        // ✅ 2. Actualizar ese formulario
        //
        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
    UPDATE Formularios SET
        Area = $area,
        Empresa = $empresa,
        ISO = $iso,
        NOM = $nom,
        Contrato = $contrato,
        Solicitud = $solicitud,
        Requerimiento = $requerimiento,
        Permiso = $permiso,
        Peticion = $peticion
    WHERE Id = $id;
";

        updateCmd.Parameters.AddWithValue("$id", formularioId);
        updateCmd.Parameters.AddWithValue("$area", Input.Area);
        updateCmd.Parameters.AddWithValue("$empresa", Input.Empresa);
        updateCmd.Parameters.AddWithValue("$iso", Input.ISO);
        updateCmd.Parameters.AddWithValue("$nom", Input.NOM);
        updateCmd.Parameters.AddWithValue("$contrato", Input.Contrato);
        updateCmd.Parameters.AddWithValue("$solicitud", Input.Solicitud);
        updateCmd.Parameters.AddWithValue("$requerimiento", Input.Requerimiento);
        updateCmd.Parameters.AddWithValue("$permiso", Input.Permiso);
        updateCmd.Parameters.AddWithValue("$peticion", Input.Peticion);

        updateCmd.ExecuteNonQuery();
        return RedirectToPage("/ZeroThird");

    }

    public class FormInput
    {
        ///Desplegables que no ocupan expresiones regular
        [Required(ErrorMessage = "Es necesario seleccionar un area ")]
        [Display(Name = "Area")]
        public string Area { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar una NOM")]
        [Display(Name = "NOM")]
        public string NOM { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar una ISO")]
        [Display(Name = "ISO")]
        public string ISO { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar un contrato")]
        [Display(Name = "Contrato")]
        public string Contrato { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar una empresa")]
        [Display(Name = "Empresa")]
        public string Empresa { get; set; } = string.Empty;
        [Required(ErrorMessage = "Es necesario seleccionar una solicitud")]
        [Display(Name = "Solicitud")]
        public string Solicitud { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar un requerimiento")]
        [Display(Name = "Requerimiento")]
        public string Requerimiento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario selecciona un permiso")]
        [Display(Name = "Permiso")]
        public string Permiso { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar una peticion")]
        [Display(Name = "Peticion")]
        public string Peticion { get; set; } = string.Empty;
        ///para guardar en db
        // [ActionName("Enviar")]
        // public IActionResult OnPostEnviar()
        // { }
    }
}







