# Email Service API

Un servizio API RESTful per l'invio di email tramite Gmail, sviluppato in ASP.NET Core. Questa API fornisce un'interfaccia semplice e robusta per inviare email da applicazioni esistenti, gestendo in modo sicuro le credenziali e fornendo log dettagliati delle operazioni.

## Caratteristiche principali

- 📧 API RESTful per l'invio di singole email o batch di email
- 📝 Supporto per email in formato HTML e testo semplice
- 📎 Gestione completa degli allegati email
- 🔒 Archiviazione sicura delle credenziali nel registro di Windows
- 📊 Sistema di logging avanzato con Serilog
- ⚙️ Configurazione flessibile per ambienti di sviluppo e produzione
- 🧪 Test unitari completi

## Architettura del progetto

Il progetto segue un'architettura a strati per garantire separazione delle responsabilità e manutenibilità del codice:

- **EmailService.API**: Livello API con i controller e configurazione
- **EmailService.Core**: Modelli, interfacce e logica di business
- **EmailService.Infrastructure**: Implementazioni concrete delle interfacce
- **EmailService.Tests**: Test unitari

## Prerequisiti

- .NET 6.0 SDK o superiore
- Visual Studio 2022 o Visual Studio Code
- Account Gmail (con "App Password" se hai l'autenticazione a due fattori)
- Permessi di amministratore per la configurazione del registro di Windows (solo in produzione)

## Configurazione

### Installazione

1. Clona il repository:
   ```bash
   git clone https://github.com/tuousername/EmailService.git
   cd EmailService
   ```

2. Ripristina i pacchetti e compila il progetto:
   ```bash
   dotnet restore
   dotnet build
   ```

### Configurazione per lo sviluppo

1. Crea un file `appsettings.json` nella cartella `src/EmailService.API` basandoti su `appsettings.example.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning",
         "EmailService": "Debug"
       }
     },
     "AllowedHosts": "*",
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "Username": "tuogmailaccount@gmail.com",
       "Password": "password_o_app_password",
       "UseSsl": true,
       "SenderName": "Nome Mittente",
       "SenderEmail": "tuogmailaccount@gmail.com"
     },
     "Serilog": {
       "MinimumLevel": {
         "Default": "Debug",
         "Override": {
           "Microsoft": "Information",
           "System": "Information"
         }
       }
     }
   }
   ```

2. Avvia l'applicazione in modalità sviluppo:
   ```bash
   cd src/EmailService.API
   dotnet run
   ```

3. L'API sarà disponibile all'indirizzo `https://localhost:5001` (o l'URL mostrato nella console).

### Configurazione per la produzione

In ambiente di produzione, le credenziali sensibili non devono essere memorizzate in file di configurazione. Il servizio utilizza il registro di Windows per memorizzare in modo sicuro le credenziali di accesso al server SMTP.

1. Esegui lo script PowerShell come amministratore:
   ```powershell
   .\scripts\ConfigureEmailCredentials.ps1 -Username "tuo-account@gmail.com" -Password "la-tua-app-password" -SenderName "Nome Mittente"
   ```

2. Questo script creerà una chiave di registro protetta in `HKLM:\SOFTWARE\YourCompany\EmailService` con accesso limitato agli amministratori, SYSTEM e all'account di servizio specifico.

3. Pubblica l'applicazione in modalità produzione:
   ```bash
   dotnet publish -c Release -o publish
   ```

4. Puoi creare un servizio Windows per eseguire l'API, utilizzando lo script PowerShell:
   ```powershell
   .\scripts\createWindowsService.ps1
   ```

## Uso dell'API

### Invio di una singola email

```http
POST /api/email/send
Content-Type: application/json

{
  "to": "destinatario@example.com",
  "subject": "Test Email",
  "body": "<h1>Titolo</h1><p>Corpo del messaggio</p>",
  "isHtml": true,
  "attachments": [
    {
      "fileName": "documento.pdf",
      "content": "base64-encoded-content",
      "contentType": "application/pdf"
    }
  ]
}
```

### Invio di multiple email in batch

```http
POST /api/email/send-batch
Content-Type: application/json

[
  {
    "to": "destinatario1@example.com",
    "subject": "Email 1",
    "body": "Corpo email 1"
  },
  {
    "to": "destinatario2@example.com",
    "subject": "Email 2",
    "body": "Corpo email 2"
  }
]
```

## Integrazione con applicazioni esistenti

### Integrazione con ASP.NET

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EmailApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public EmailApiClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var emailData = new
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        };

        var json = JsonSerializer.Serialize(emailData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_baseUrl}/api/email/send", content);
        response.EnsureSuccessStatusCode();
    }
}
```

### Integrazione con ASP Classic

```vb
<%
' Esempio di integrazione con ASP Classic
Function SendEmail(toEmail, subject, body)
    ' Prepara i dati JSON per la richiesta
    Dim jsonData
    jsonData = "{" & _
        """to"": """ & toEmail & """," & _
        """subject"": """ & subject & """," & _
        """body"": """ & body & """," & _
        """isHtml"": true" & _
    "}"
    
    ' Crea l'oggetto per la richiesta HTTP
    Set http = Server.CreateObject("MSXML2.ServerXMLHTTP")
    http.Open "POST", "http://tuo-server:5000/api/email/send", False
    http.setRequestHeader "Content-Type", "application/json"
    
    ' Invia la richiesta
    http.send jsonData
    
    ' Verifica la risposta
    If http.status = 200 Then
        SendEmail = True
    Else
        SendEmail = False
        ' Registra l'errore
        Response.Write "Errore nell'invio dell'email: " & http.responseText
    End If
    
    Set http = Nothing
End Function
%>
```

## Configurazione avanzata

### Ambienti e variabili di configurazione

L'applicazione ha diverse configurazioni per gli ambienti di sviluppo e produzione:

| Ambiente | Fonte configurazione | File/Percorso |
|----------|----------------------|---------------|
| Sviluppo | File JSON | `src/EmailService.API/appsettings.json` |
| Produzione | Registro di Windows | `HKLM:\SOFTWARE\YourCompany\EmailService` |

### Configurazione del logging

I log vengono salvati nella cartella `logs` all'interno della directory dell'applicazione. È possibile modificare le impostazioni di logging in `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",  // Cambia in Debug per log più dettagliati
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "logs/emailservice-.log",
        "rollingInterval": "Day"
      }
    }
  ]
}
```

### CORS e sicurezza

Per modificare le origini consentite per le richieste CORS, modifica la configurazione in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        corsBuilder => corsBuilder
            .WithOrigins("http://tuo-server-app.com")  // Aggiungi qui i domini consentiti
            .AllowAnyMethod()
            .AllowAnyHeader());
});
```

## Migrare da development a production

### Passaggi per la migrazione

1. **Configura le credenziali nel registro** usando lo script PowerShell:
   ```powershell
   .\scripts\ConfigureEmailCredentials.ps1 -Username "account@gmail.com" -Password "app-password"
   ```

2. **Pubblica l'applicazione** in modalità Release:
   ```bash
   dotnet publish -c Release -o publish
   ```

3. **Crea un servizio Windows** per eseguire l'API:
   ```powershell
   .\scripts\createWindowsService.ps1
   ```

4. **Verifica i permessi** per assicurarti che l'account del servizio Windows abbia accesso in lettura alla chiave di registro.

### Valori da modificare

Quando passi da sviluppo a produzione, considera di modificare:

1. **Endpoint CORS**: Aggiorna con i domini di produzione
2. **Livelli di logging**: In produzione, imposta livelli più alti (Information o Warning)
3. **Credenziali email**: Usa un account di servizio dedicato per l'invio email

## Test

Esegui i test unitari con:

```bash
cd src/EmailService.Tests
dotnet test
```

## Licenza

[MIT](LICENSE)
