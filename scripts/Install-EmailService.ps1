# Install-EmailService.ps1
# Script per l'installazione del servizio EmailAPI come servizio Windows
# Deve essere eseguito con privilegi amministrativi
param(
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\ServicesApp\EmailService",
    
    [Parameter(Mandatory=$false)]
    [string]$BinaryPath = ".\publish",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "EmailServiceAPI",
    
    [Parameter(Mandatory=$false)]
    [string]$DisplayName = "Email Service API",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceUser = "LocalSystem",
    
    [Parameter(Mandatory=$false)]
    [string]$ServicePassword = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$Description = "Servizio API per l'invio di email tramite SMTP",
    
    [Parameter(Mandatory=$false)]
    [switch]$AutoStart = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$ForceReinstall = $false
)

function Write-StepHeader ($message) {
    Write-Host "`n== $message ==" -ForegroundColor Cyan
}

function Write-Success ($message) {
    Write-Host $message -ForegroundColor Green
}

function Write-Warning ($message) {
    Write-Host $message -ForegroundColor Yellow
}

function Write-Error ($message) {
    Write-Host $message -ForegroundColor Red
}

# Verifica privilegi amministrativi
Write-StepHeader "Verifica privilegi amministrativi"
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Questo script deve essere eseguito come amministratore."
    exit 1
}
Write-Success "Script eseguito con privilegi amministrativi."

# Verifica se il servizio esiste già
Write-StepHeader "Controllo del servizio esistente"
$serviceExists = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($serviceExists) {
    Write-Warning "Il servizio '$ServiceName' esiste già."
    
    if ($ForceReinstall) {
        Write-Warning "Flag ForceReinstall specificato. Il servizio esistente verrà rimosso."
        
        # Arresta il servizio se in esecuzione
        if ($serviceExists.Status -eq "Running") {
            Write-Host "Arresto del servizio in corso..." -ForegroundColor Yellow
            Stop-Service -Name $ServiceName -Force
            Start-Sleep -Seconds 2
        }
        
        # Rimuovi il servizio
        Write-Host "Rimozione del servizio in corso..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
        Write-Success "Servizio rimosso con successo."
    } else {
        Write-Error "Il servizio esiste già e il flag ForceReinstall non è specificato. Uscita."
        exit 1
    }
}

# Crea la cartella di installazione se non esiste
Write-StepHeader "Preparazione della cartella di installazione"
if (-not (Test-Path $InstallPath)) {
    Write-Host "Creazione della cartella $InstallPath..."
    New-Item -Path $InstallPath -ItemType Directory -Force | Out-Null
    Write-Success "Cartella creata."
} else {
    Write-Host "La cartella $InstallPath esiste già." -ForegroundColor Yellow
}

# Copia i file dell'applicazione
Write-StepHeader "Copia dei file dell'applicazione"
if (-not (Test-Path $BinaryPath)) {
    Write-Error "Il percorso sorgente $BinaryPath non esiste."
    exit 1
}

Write-Host "Copia dei file da $BinaryPath a $InstallPath in corso..."
Copy-Item -Path "$BinaryPath\*" -Destination $InstallPath -Recurse -Force
Write-Success "File copiati con successo."

# Crea directory per i log se non esiste
$logPath = Join-Path -Path $InstallPath -ChildPath "logs"
if (-not (Test-Path $logPath)) {
    Write-Host "Creazione della cartella logs..."
    New-Item -Path $logPath -ItemType Directory -Force | Out-Null
    Write-Success "Cartella logs creata."
}

# Imposta le autorizzazioni
Write-StepHeader "Configurazione delle autorizzazioni"
$acl = Get-Acl $InstallPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($ServiceUser, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $InstallPath $acl
Write-Success "Autorizzazioni impostate per $ServiceUser."

# Crea il servizio Windows
Write-StepHeader "Creazione del servizio Windows"
$binaryPathName = "`"$InstallPath\EmailService.API.exe`" --contentRoot `"$InstallPath`""

$startupType = "Automatic"
if (-not $AutoStart) {
    $startupType = "Manual"
}

if ($ServiceUser -eq "LocalSystem") {
    New-Service -Name $ServiceName -BinaryPathName $binaryPathName -DisplayName $DisplayName -Description $Description -StartupType $startupType | Out-Null
} else {
    # Se viene fornito un account utente specifico
    if ([string]::IsNullOrEmpty($ServicePassword)) {
        Write-Error "È necessaria una password per l'account servizio $ServiceUser."
        exit 1
    }
    
    New-Service -Name $ServiceName -BinaryPathName $binaryPathName -DisplayName $DisplayName -Description $Description -StartupType $startupType -Credential (New-Object System.Management.Automation.PSCredential($ServiceUser, (ConvertTo-SecureString $ServicePassword -AsPlainText -Force))) | Out-Null
}

Write-Success "Servizio $ServiceName creato con successo."

# Avvia il servizio se richiesto
if ($AutoStart) {
    Write-StepHeader "Avvio del servizio"
    try {
        Start-Service -Name $ServiceName
        $status = Get-Service -Name $ServiceName
        Write-Success "Servizio avviato. Stato attuale: $($status.Status)"
    } catch {
        Write-Error "Errore durante l'avvio del servizio: $_"
        Write-Warning "Verificare i log di sistema per ulteriori dettagli."
    }
}

# Mostra riepilogo finale
Write-StepHeader "Installazione completata"
Write-Host "Informazioni di installazione:" -ForegroundColor Cyan
Write-Host "- Nome servizio: $ServiceName"
Write-Host "- Percorso installazione: $InstallPath"
Write-Host "- Account servizio: $ServiceUser"
Write-Host "- Avvio automatico: $AutoStart"
Write-Host "- Eseguibile: $binaryPathName"

# Verifica nuovamente lo stato
$finalStatus = Get-Service -Name $ServiceName
Write-Host "Stato finale del servizio: $($finalStatus.Status)" -ForegroundColor Cyan

# Mostra istruzioni per la gestione
Write-Host "`nPer gestire il servizio manualmente, usa i seguenti comandi:" -ForegroundColor Cyan
Write-Host "- Start-Service -Name $ServiceName    # Avvia il servizio"
Write-Host "- Stop-Service -Name $ServiceName     # Arresta il servizio"
Write-Host "- Restart-Service -Name $ServiceName  # Riavvia il servizio"
Write-Host "- Get-Service -Name $ServiceName      # Mostra lo stato del servizio"