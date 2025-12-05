

using System.Text;
using System.Text.Json;

public class EmailService
{
  private readonly IConfiguration _config;

  public EmailService(IConfiguration config)
  {
    _config = config;
  }

  public async Task EnviarCorreoConfirmacion(string correoDestino, string token)
  {
    string apiKey = _config["Resend:ApiKey"];

    // Leer el BaseUrl desde appsettings.json
    string baseUrl = _config["AppSettings:BaseUrl"];

    // Usar el token REAL que viene del registro
    string urlConfirmacion = $"{baseUrl}/Confirmar?token={Uri.EscapeDataString(token)}";


    var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

    var data = new
    {
      from = "onboarding@resend.dev",
      to = new[] { correoDestino },
      subject = "Confirma tu correo",
      html = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <style>
    body {{
      font-family: Arial, sans-serif;
      background-color: #f4f4f7;
      margin: 0;
      padding: 0;
    }}
    .container {{
      max-width: 600px;
      margin: 40px auto;
      background: #ffffff;
      border-radius: 8px;
      box-shadow: 0 2px 5px rgba(0,0,0,0.1);
      padding: 30px;
    }}
    h2 {{
      color: #333333;
    }}
    p {{
      color: #555555;
      line-height: 1.5;
    }}
    .button {{
      display: inline-block;
      margin-top: 20px;
      padding: 12px 20px;
      background-color: #9d2449
;
      color: #ffffff !important;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
    }}
    .footer {{
      margin-top: 30px;
      font-size: 12px;
      color: #999999;
      text-align: center;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <h2>Confirma tu correo electrónico</h2>
    <p>Hola,</p>
    <p>Gracias por registrarte. Para activar tu cuenta, por favor confirma tu dirección de correo electrónico haciendo clic en el siguiente botón:</p>
    <p><a href='{urlConfirmacion}' class='button'>Confirmar correo</a></p>
    <p>Si el botón no funciona, copia y pega esta URL en tu navegador:</p>
    <p>{urlConfirmacion}</p>
    <div class='footer'>
      <p>© {DateTime.Now.Year}</p>
    </div>
  </div>
</body>
</html>
"
    };

    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await http.PostAsync("https://api.resend.com/emails", content);

    Console.WriteLine("Respuesta Resend: " + response.StatusCode);
    Console.WriteLine("API KEY USADA: " + apiKey);

  }
  public async Task EnviarCorreoGenerico(string correoDestino, string asunto, string htmlContenido)
  {
    string apiKey = _config["Resend:ApiKey"];
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

    var data = new
    {
      from = "onboarding@resend.dev",
      to = new[] { correoDestino },
      subject = asunto,
      html = htmlContenido
    };

    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
      var response = await http.PostAsync("https://api.resend.com/emails", content);
      response.EnsureSuccessStatusCode();
      Console.WriteLine("Correo enviado correctamente");
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error enviando correo: " + ex.Message);
    }
  }


  //email de notificacion al completar el formulario y enviarlo
  public async Task EnviarNotificacion(string correoDestino, string asunto, string htmlContenido)
  {
    string apiKey = _config["Resend:ApiKey"];
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

    var data = new
    {
      from = "onboarding@resend.dev",
      to = new[] { correoDestino },
      subject = asunto,
      html = htmlContenido
    };

    var json = JsonSerializer.Serialize(data);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
      var response = await http.PostAsync("https://api.resend.com/emails", content);
      response.EnsureSuccessStatusCode();
      Console.WriteLine("Correo enviado correctamente");
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error enviando correo: " + ex.Message);
    }
  }
}


















