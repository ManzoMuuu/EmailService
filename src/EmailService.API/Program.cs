using EmailService.API.Middleware;
using EmailService.Core.Interfaces;
using EmailService.Core.Models;
using EmailService.Infrastructure.Services;
using EmailService.Infrastructure.Configuration;
using Microsoft.Win32;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Email Service API", 
        Version = "v1",
        Description = "API per l'invio di email tramite Gmail",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });
});

// Configurazione del registro di Windows in ambiente di produzione
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddWindowsRegistry(
        "SOFTWARE\\YourCompany\\EmailService",
        RegistryHive.LocalMachine);
}

// Configurazione Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ApplicationName", "EmailService.API")
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/emailservice-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({ThreadId}) {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Configurazione dei servizi dell'applicazione
builder.Services.AddControllers();

// Configurazione del servizio email basata sull'ambiente
if (builder.Environment.IsProduction())
{
    // Configura la dipendenza dal registro di Windows
    builder.Services.Configure<EmailConfig>(options =>
    {
        options.SmtpServer = builder.Configuration["SmtpServer"] ?? "smtp.gmail.com";
        options.SmtpPort = int.TryParse(builder.Configuration["SmtpPort"], out int port) ? port : 587;
        options.Username = builder.Configuration["Username"] ?? "";
        options.Password = builder.Configuration["Password"] ?? "";
        options.UseSsl = builder.Configuration["UseSsl"] == "1";
        options.SenderName = builder.Configuration["SenderName"] ?? "Email Service";
        options.SenderEmail = builder.Configuration["SenderEmail"] ?? "";
    });
}
else
{
    // In development, usa la configurazione dal file appsettings.json
    builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailSettings"));
}

// Registrazione dei servizi dell'applicazione
builder.Services.AddScoped<IEmailService, GmailService>();
builder.Services.AddScoped<IEmailAttachmentService, EmailAttachmentService>();

// CORS non è necessario poiché tutto è nello stesso server
// Ma lo configuriamo comunque per lo sviluppo e futuri scenari
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalOnly",
        corsBuilder => corsBuilder
            .SetIsOriginAllowed(_ => true) // Consenti tutte le origini locali
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Il rate limiting è stato rimosso perché non necessario in ambiente locale
// Se in futuro sarà necessario, installare il pacchetto appropriato e riconfigurarlo

// Costruzione dell'applicazione
var app = builder.Build();

// Configurazione della pipeline di richiesta HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aggiunta del middleware di tracciamento delle richieste personalizzato
app.UseRequestTracking();

// Middleware per il logging delle richieste - configurazione semplificata per ambiente locale
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        // Cattura solo l'applicazione richiedente se specificata nell'header
        var appName = httpContext.Items["ApplicationName"]?.ToString();
        if (!string.IsNullOrEmpty(appName))
        {
            diagnosticContext.Set("ApplicationName", appName);
        }
    };
});

app.UseCors("LocalOnly");
// app.UseRateLimiter(); // Rimosso perché il rate limiting è stato disabilitato
app.UseAuthorization();
app.MapControllers();

// Endpoint per il controllo di salute dell'applicazione
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .AllowAnonymous();

// Avvio dell'applicazione con gestione eccezioni
try
{
    Log.Information("Avvio dell'applicazione Email Service API");
    app.Run();
    Log.Information("Applicazione terminata normalmente");
}
catch (Exception ex)
{
    Log.Fatal(ex, "L'applicazione è terminata in modo non previsto");
}
finally
{
    Log.CloseAndFlush();
}