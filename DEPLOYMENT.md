# Guida al Deployment di Email Service API

Questa guida fornisce istruzioni dettagliate per il deployment di Email Service API in diversi ambienti.

## Contenuti
- [Requisiti di sistema](#requisiti-di-sistema)
- [Deployment come servizio Windows](#deployment-come-servizio-windows)
- [Deployment con Docker](#deployment-con-docker)
- [Configurazione in ambienti di produzione](#configurazione-in-ambienti-di-produzione)
- [Integrazione con IIS](#integrazione-con-iis)
- [Monitoraggio e logging](#monitoraggio-e-logging)
- [Troubleshooting](#troubleshooting)

## Requisiti di sistema

### Per deployment come servizio Windows
- Windows Server 2016/2019/2022 o Windows 10/11
- .NET 8.0 Runtime installato
- PowerShell 5.1 o superiore
- Accesso amministrativo al server
- Porta TCP disponibile (default: 5001)

### Per deployment con Docker
- Docker Engine 19.03 o superiore
- Docker Compose (opzionale, per orchestrazione semplificata)
- 512 MB di RAM minimo (consigliato: 1 GB)
- Spazio su disco: almeno 150 MB

## Deployment come servizio Windows

### Passo 1: Pubblicazione dell'applicazione
Utilizzare lo script di pubblicazione fornito:

```powershell
# Esegui dalla directory principale del progetto
.\scripts\Publish-EmailService.ps1 -OutputPath "publish"
```

Lo script genera una build ottimizzata con le seguenti caratteristiche:
- Compilazione in modalità Release
- Ottimizzazione ReadyToRun per migliorare i tempi di avvio
- Pubblicazione come file singolo per semplificare la distribuzione
- Output nella cartella "publish"

### Passo 2: Configurazione dell'ambiente
Configurare l'ambiente per il servizio con lo script dedicato:

```powershell
# Esegui con privilegi amministrativi
.\scripts\Configure-Environment.ps1 -Username "tuo-account@gmail.com" -Password "tua-app-password"
```

Questo script:
- Configura il registro di Windows con le credenziali SMTP
- Crea regole del firewall per la porta API
- Verifica i prerequisiti del sistema

### Passo 3: Installazione del servizio Windows
Installare l'applicazione come servizio Windows:

```powershell
# Esegui con privilegi amministrativi
.\scripts\Install-EmailService.ps1 -BinaryPath "publish" -ServiceUser "LocalSystem"
```

Opzioni disponibili:
- `-InstallPath`: Percorso di installazione (default: "C:\ServicesApp\EmailService")
- `-ServiceName`: Nome del servizio Windows (default: "EmailServiceAPI")
- `-AutoStart`: Avvia automaticamente il servizio (default: true)
- `-ForceReinstall`: Rimuove un servizio esistente se presente

### Passo 4: Verifica dell'installazione
Verificare che il servizio sia stato installato correttamente:

```powershell
Get-Service -Name "EmailServiceAPI"
```

L'output dovrebbe mostrare il servizio in stato "Running".

### Gestione del servizio
Comandi utili per gestire il servizio:

```powershell
# Avvia il servizio
Start-Service -Name "EmailServiceAPI"

# Arresta il servizio
Stop-Service -Name "EmailServiceAPI"

# Riavvia il servizio
Restart-Service -Name "EmailServiceAPI"

# Visualizza lo stato del servizio
Get-Service -Name "EmailServiceAPI"
```

## Deployment con Docker

### Passo 1: Costruzione dell'immagine Docker
Costruire l'immagine Docker dal Dockerfile fornito:

```bash
# Dalla directory principale del progetto
docker build -t emailservice:latest .
```

### Passo 2: Esecuzione del container
Eseguire il container con le credenziali appropriate:

```bash
docker run -d -p 5001:80 \
  -e EMAILAPI_EmailSettings__Username="tua-email@gmail.com" \
  -e EMAILAPI_EmailSettings__Password="tua-password-app" \
  -e EMAILAPI_EmailSettings__SenderEmail="tua-email@gmail.com" \
  --name emailservice \
  emailservice:latest
```

### Passo 3: Utilizzo di Docker Compose (alternativa)
Alternativamente, utilizzare Docker Compose per un deployment più configurabile:

1. Creare un file `.env` nella directory principale:
```
EMAIL_USERNAME=tua-email@gmail.com
EMAIL_PASSWORD=tua-password-app
EMAIL_SENDER=tua-email@gmail.com
```

2. Eseguire Docker Compose:
```bash
docker-compose up -d
```

### Gestione del container
Comandi utili per gestire il container:

```bash
# Visualizza i log del container
docker logs emailservice

# Ferma il container
docker stop emailservice

# Riavvia il container
docker restart emailservice

# Rimuovi il container
docker rm -f emailservice
```

## Configurazione in ambienti di produzione

### Sicurezza delle credenziali
In produzione, evitare di esporre le credenziali:

1. **Con Windows**: Utilizzare il registro di Windows come nell'esempio fornito dallo script `Configure-Environment.ps1`

2. **Con Docker**: Utilizzare Docker Secrets o un sistema di gestione delle credenziali come Azure Key Vault

3. **Con Kubernetes**: Utilizzare Kubernetes Secrets per gestire le credenziali in modo sicuro

### Configurazione SSL/TLS
Per abilitare HTTPS:

1. **Con Windows**: Configurare un certificato SSL con il comando:
```powershell
# Installazione di un certificato
$cert = New-SelfSignedCertificate -DnsName "emailservice.tuodominio.com" -CertStoreLocation "cert:\LocalMachine\My"
$thumbprint = $cert.Thumbprint
netsh http add sslcert ipport=0.0.0.0:5001 certhash=$thumbprint appid="{00000000-0000-0000-0000-000000000000}"
```

2. **Con Docker/Kubernetes**: Utilizzare un ingress controller o un proxy inverso come Nginx/Traefik per terminare SSL

## Integrazione con IIS

Per integrare il servizio con IIS esistente:

### Opzione 1: Proxy inverso IIS
1. Installare il modulo URL Rewrite in IIS
2. Configurare una regola di inoltro:

```xml
<rewrite>
  <rules>
    <rule name="Forward to EmailAPI">
      <match url="api/email/(.*)" />
      <action type="Rewrite" url="http://localhost:5001/api/email/{R:1}" />
    </rule>
  </rules>
</rewrite>
```

### Opzione 2: Configurazione come applicazione IIS
1. Creare un nuovo sito o un'applicazione in IIS
2. Configurare il pool di applicazioni per utilizzare .NET CLR v4.0.30319 in modalità No Managed Code
3. Impostare la cartella fisica sul percorso di pubblicazione dell'API
4. Configurare il binding per la porta desiderata

## Monitoraggio e logging

### File di log
I log vengono scritti nella cartella `logs` all'interno della directory dell'applicazione:

- Windows: `C:\ServicesApp\EmailService\logs\` (o il tuo percorso di installazione)
- Docker: `/app/logs/` all'interno del container (montato come volume)

### Configurazione del logging
La configurazione dei log può essere regolata in `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

Livelli disponibili (dal meno al più dettagliato):
- `Fatal`
- `Error`
- `Warning`
- `Information`
- `Debug`
- `Verbose`

### Integrazione con sistemi di monitoraggio
Per integrare con sistemi esterni di monitoraggio:

1. **Windows Event Log**:
```json
"Serilog": {
  "WriteTo": [
    {
      "Name": "EventLog",
      "Args": {
        "source": "EmailServiceAPI",
        "logName": "Application"
      }
    }
  ]
}
```

2. **Elasticsearch/Kibana**:
Installare il pacchetto `Serilog.Sinks.Elasticsearch` e configurare:
```json
"Serilog": {
  "WriteTo": [
    {
      "Name": "Elasticsearch",
      "Args": {
        "nodeUris": "http://elasticsearch:9200"
      }
    }
  ]
}
```

## Troubleshooting

### Problemi comuni e soluzioni

#### Il servizio non si avvia
1. Verificare che .NET 8.0 Runtime sia installato
2. Controllare i log di Windows nella sezione "Application" di Event Viewer
3. Verificare che la porta configurata non sia già in uso

#### Errori di credenziali SMTP
1. Assicurarsi che la password sia corretta
2. Per Gmail, utilizzare una "App Password" se l'autenticazione a due fattori è attiva
3. Verificare che l'account email non abbia restrizioni di sicurezza

#### Problemi di accesso al registro di Windows
1. Verificare che l'account del servizio abbia autorizzazioni di lettura per la chiave del registro
2. Utilizzare `regedit` per controllare l'esistenza della chiave `HKLM:\SOFTWARE\YourCompany\EmailService`
3. Ripristinare le autorizzazioni con lo script `Configure-Environment.ps1`

#### Problemi di firewall
1. Verificare che la porta sia aperta nel firewall:
```powershell
Get-NetFirewallRule -DisplayName "Email Service API"
```
2. Controllare che non ci siano altre regole che blocchino la comunicazione

### Come ottenere supporto
Per ulteriori problemi, consultare:
1. I log dell'applicazione nella cartella `logs`
2. L'Event Viewer di Windows
3. Aprire un issue nel repository del progetto con una descrizione dettagliata del problema