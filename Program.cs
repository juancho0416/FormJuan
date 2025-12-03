
var builder = WebApplication.CreateBuilder(args);

// Servicios necesarios
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddScoped<EmailService>();


// Registrar SignalR
builder.Services.AddSignalR();

// Habilitar sesiones
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// Configuración del pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Activar sesiones ANTES de Authorization
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();




