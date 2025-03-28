# Publish-EmailService.ps1
# Script per generare una build di pubblicazione dell'API EmailService
param(
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "publish",
    
    [Parameter(Mandatory=$false)]
    [string]$Framework = "net8.0",
    
    [Parameter(Mandatory=$false)]
    [string]$Runtime = "win-x64",
    
    [Parameter(Mandatory=$false)]
    [switch]$SelfContained = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$PublishSingleFile = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$PublishReadyToRun = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$PublishTrimmed = $false
)

function Write-StepHeader ($message) {
    Write-Host "`n== $message ==" -ForegroundColor Cyan
}

function Write-Success ($message) {
    Write-Host $message -ForegroundColor Green
}

# Definisci il percorso del progetto
$projectPath = "src\EmailService.API\EmailService.API.csproj"

# Verifica che il file di progetto esista
if (-not (Test-Path $projectPath)) {
    Write-Error "Il file di progetto non esiste: $projectPath"
    exit 1
}

# Pulisci le build precedenti
Write-StepHeader "Pulizia delle build precedenti"
dotnet clean $projectPath -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Errore durante la pulizia del progetto."
    exit 1
}
Write-Success "Pulizia completata."

# Ripristina i pacchetti
Write-StepHeader "Ripristino dei pacchetti NuGet"
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "Errore durante il ripristino dei pacchetti."
    exit 1
}
Write-Success "Pacchetti ripristinati con successo."

# Costruisci gli argomenti di pubblicazione
$publishArgs = @(
    "publish",
    $projectPath,
    "-c", $Configuration,
    "-f", $Framework,
    "-o", $OutputPath,
    "--no-restore"
)

if ($Runtime) {
    $publishArgs += "-r"
    $publishArgs += $Runtime
}

if ($SelfContained) {
    $publishArgs += "--self-contained"
    $publishArgs += "true"
}

if ($PublishSingleFile) {
    $publishArgs += "-p:PublishSingleFile=true"
}

if ($PublishReadyToRun) {
    $publishArgs += "-p:PublishReadyToRun=true"
}

if ($PublishTrimmed) {
    $publishArgs += "-p:PublishTrimmed=true"
}

# Esegui il comando di pubblicazione
Write-StepHeader "Pubblicazione del progetto"
Write-Host "Esecuzione del comando: dotnet $($publishArgs -join ' ')" -ForegroundColor Gray
& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Errore durante la pubblicazione del progetto."
    exit 1
}

Write-Success "Pubblicazione completata con successo nella cartella: $OutputPath"

# Verifica i file pubblicati
$exePath = Join-Path -Path $OutputPath -ChildPath "EmailService.API.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    Write-Host "File eseguibile: $($fileInfo.FullName)"
    Write-Host "Dimensione: $([math]::Round($fileInfo.Length / 1MB, 2)) MB"
    Write-Host "Data: $($fileInfo.LastWriteTime)"
} else {
    Write-Warning "File eseguibile non trovato nel percorso di output."
}

# Suggerimenti finali
Write-StepHeader "Prossimi passaggi"
Write-Host "Per installare l'applicazione come servizio Windows, esegui:" -ForegroundColor Cyan
Write-Host ".\Install-EmailService.ps1 -BinaryPath `"$OutputPath`"" -ForegroundColor Yellow

# Ritorna al prompt
Write-Host "`nPubblicazione completata. Premi un tasto per uscire..." -ForegroundColor Green
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")