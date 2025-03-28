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
builder.Services.AddSwaggerGen();

builder.Configuration.AddWindowsRegistry(
    "SOFTWARE\\YourCompany\\EmailService",
    RegistryHive.LocalMachine);

// Configurazione Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/emailservice-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({ThreadId}) {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();


// Resto della configurazione
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure email service
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
builder.Services.AddScoped<IEmailService, GmailService>();
builder.Services.AddScoped<IEmailAttachmentService, EmailAttachmentService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        corsBuilder => corsBuilder
            .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // Aggiunge logging delle richieste HTTP

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.MapControllers();

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

