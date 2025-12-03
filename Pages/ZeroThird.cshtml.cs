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

public class ZeroThirdModel : PageModel
{


    [BindProperty]
    public FormInput Input { get; set; } = new FormInput();



    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Guardar en TempData (si lo sigues usando)
        TempData["Regulacion"] = Input.Regulacion;
        TempData["Ley"] = Input.Ley;
        TempData["Articulo"] = Input.Articulo;
        TempData["Parrafo"] = Input.Parrafo;

        // ✅ Obtener correo del usuario logueado
        string correo = HttpContext.Session.GetString("correo") ?? string.Empty;

        if (string.IsNullOrEmpty(correo))
        {
            ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario. Inicia sesión nuevamente.");
            return Page();
        }

        using var connection = new SqliteConnection("Data Source=usuarios.db");
        connection.Open();

        //
        // ✅ 1. Obtener el ID del último formulario del usuario
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
        // ✅ 2. Actualizar ese formulario con los datos de ZeroThird
        //
        var updateCmd = connection.CreateCommand();
        updateCmd.CommandText = @"
        UPDATE Formularios SET
            Regulacion = $regulacion,
            Ley = $ley,
            Articulo = $articulo,
            Parrafo = $parrafo
        WHERE Id = $id;
    ";

        updateCmd.Parameters.AddWithValue("$id", formularioId);
        updateCmd.Parameters.AddWithValue("$regulacion", Input.Regulacion);
        updateCmd.Parameters.AddWithValue("$ley", Input.Ley);
        updateCmd.Parameters.AddWithValue("$articulo", Input.Articulo);
        updateCmd.Parameters.AddWithValue("$parrafo", Input.Parrafo);

        updateCmd.ExecuteNonQuery();
        // Redirige  a third
        return RedirectToPage("TwoNine");


    }

    public class FormInput
    {
        ///Desplegables que no ocupan expresiones regular

        [Required(ErrorMessage = "Es necesario seleccionar un area")]
        [Display(Name = "Seccion")]
        public string Seccion { get; set; } = string.Empty;
        [Required(ErrorMessage = "Es necesario seleccionar una regulacion")]
        [Display(Name = "Regulacion")]
        public string Regulacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar una ley")]
        [Display(Name = "Ley")]
        public string Ley { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar un articulo")]
        [Display(Name = "Articulo")]
        public string Articulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Es necesario seleccionar una parrafo")]
        [Display(Name = "Parrafo")]
        public string Parrafo { get; set; } = string.Empty;

    }

}






