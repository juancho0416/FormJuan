
using Resend;
using form.Services;      // AuditoriaService y PasswordResetEmailService
using form.Models;        // AppSettings
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// AppSettings (BaseUrl)
// -------------------------

// / Inyección de AuditoriaService(usa cadena de conexión de appsettings o fallback)
var connString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=usuarios.db";
builder.Services.AddScoped<AuditoriaService>(_ => new AuditoriaService(connString));

var appSettings = new AppSettings();
builder.Configuration.GetSection("AppSettings").Bind(appSettings);
// Normaliza: quita '/' final si viene
if (!string.IsNullOrWhiteSpace(appSettings.BaseUrl))
    appSettings.BaseUrl = appSettings.BaseUrl.TrimEnd('/');
builder.Services.AddSingleton(appSettings);

// -------------------------
// Servicios
// -------------------------
builder.Services.AddRazorPages();

// Cache para Session (mínimo viable)
builder.Services.AddDistributedMemoryCache();

// HttpClient general
builder.Services.AddHttpClient();

// EmailService (deja SOLO una vida útil; aquí Scoped)
builder.Services.AddScoped<EmailService>();

///notificacion service
builder.Services.AddScoped<NotificacionService>(_ =>
    new NotificacionService(builder.Configuration.GetConnectionString("Default") ?? "Data Source=usuarios.db"));

// Auditoría (tu clase actual) con conexión centralizada
// var cs = builder.Configuration.GetConnectionString("DefaultConnection")
//          ?? "Data Source=usuarios.db;Cache=Shared";
// builder.Services.AddScoped<AuditoriaService>(_ => new AuditoriaService(cs));

// Resend SDK (DI oficial) - lee Resend:ApiKey
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10); // evita cuelgues prolongados
});
builder.Services.Configure<ResendClientOptions>(o =>
{
    var apiToken = builder.Configuration["Resend:ApiKey"]
                    ?? Environment.GetEnvironmentVariable("RESEND_APITOKEN");

    if (string.IsNullOrWhiteSpace(apiToken))
        throw new InvalidOperationException("Falta Resend ApiKey (appsettings: Resend:ApiKey) o variable RESEND_APITOKEN.");

    o.ApiToken = apiToken; // El SDK usa 'ApiToken' internamente
});
builder.Services.AddTransient<IResend, ResendClient>();

// Servicio de restablecimiento (aparte del EmailService)
builder.Services.AddTransient<PasswordResetEmailService>();

// SignalR (si lo vas a usar)
builder.Services.AddSignalR();

// Habilitar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// -------------------------
// Pipeline
// -------------------------
// (Opcional) Respeta X-Forwarded-* (útil con ngrok/proxy)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session ANTES de los endpoints
app.UseSession();

// Si añades autenticación en el futuro:
// app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

// SignalR Hub (si existe, descomenta y crea tu Hub)
// app.MapHub<form.Hubs.NotificacionesHub>("/notificaciones");

app.Run();



