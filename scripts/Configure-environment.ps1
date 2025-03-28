# Configure-Environment.ps1
# Script per configurare l'ambiente operativo per EmailService
param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production",
    
    [Parameter(Mandatory=$false)]
    [switch]$ConfigureRegistry = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$ConfigureFirewall = $true,
    
    [Parameter(Mandatory=$false)]
    [int]$ApiPort = 5001,
    
    [Parameter(Mandatory=$false)]
    [string]$SmtpServer = "smtp.gmail.com",
    
    [Parameter(Mandatory=$false)]
    [int]$SmtpPort = 587,
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [string]$SenderName = "Email Service",
    
    [Parameter(Mandatory=$false)]
    [bool]$UseSsl = $true
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

# Configura il registro di Windows
if ($ConfigureRegistry) {
    Write-StepHeader "Configurazione del registro di Windows"
    
    # Richiedi le credenziali se non sono state fornite
    if ([string]::IsNullOrEmpty($Username)) {
        $Username = Read-Host -Prompt "Inserisci l'username Gmail/SMTP"
    }
    
    if ([string]::IsNullOrEmpty($Password)) {
        $securePassword = Read-Host -Prompt "Inserisci la password (App Password per Gmail)" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
        $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    }
    
    # Imposta il percorso del registro
    $registryPath = "SOFTWARE\YourCompany\EmailService"

    # Crea la chiave se non esiste
    if (-not (Test-Path "HKLM:\$registryPath")) {
        New-Item -Path "HKLM:\$registryPath" -Force | Out-Null
        Write-Success "Chiave di registro creata."
    }
    
    # Imposta i valori
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "Username" -Value $Username
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "Password" -Value $Password
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SenderName" -Value $SenderName
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SenderEmail" -Value $Username
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SmtpServer" -Value $SmtpServer
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SmtpPort" -Value $SmtpPort
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "UseSsl" -Value $([int]$UseSsl)
    
    # Restringe l'accesso alla chiave
    $acl = Get-Acl "HKLM:\$registryPath"
    $acl.SetAccessRuleProtection($true, $false)
    
    $adminRule = New-Object System.Security.AccessControl.RegistryAccessRule(
        "Administrators", 
        "FullControl", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow"
    )
    
    $systemRule = New-Object System.Security.AccessControl.RegistryAccessRule(
        "SYSTEM", 
        "FullControl", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow"
    )
    
    $serviceAccountRule = New-Object System.Security.AccessControl.RegistryAccessRule(
        "NetworkService", 
        "ReadKey", 
        "ContainerInherit,ObjectInherit", 
        "None", 
        "Allow"
    )
    
    $acl.AddAccessRule($adminRule)
    $acl.AddAccessRule($systemRule)
    $acl.AddAccessRule($serviceAccountRule)
    
    Set-Acl "HKLM:\$registryPath" $acl
    
    Write-Success "Registro di Windows configurato."
}

# Configura il firewall di Windows
if ($ConfigureFirewall) {
    Write-StepHeader "Configurazione del firewall"
    
    $firewallRuleName = "EmailService API"
    
    # Verifica se la regola esiste già
    $existingRule = Get-NetFirewallRule -DisplayName $firewallRuleName -ErrorAction SilentlyContinue
    
    if ($existingRule) {
        Write-Warning "La regola del firewall '$firewallRuleName' esiste già. Rimozione in corso..."
        Remove-NetFirewallRule -DisplayName $firewallRuleName
    }
    
    # Crea la regola del firewall
    New-NetFirewallRule -DisplayName $firewallRuleName -Direction Inbound -Protocol TCP -LocalPort $ApiPort -Action Allow -Profile Domain,Private | Out-Null
    
    Write-Success "Regola del firewall creata per la porta $ApiPort."
}

# Verifica di IIS
$iisInstalled = Get-Service -Name "W3SVC" -ErrorAction SilentlyContinue
if (-not $iisInstalled) {
    Write-Warning "IIS non sembra essere installato. Email Service API è progettato per funzionare con IIS."
    Write-Warning "Consiglio di installare IIS con il seguente comando PowerShell (richiede privilegi amministrativi):"
    Write-Host "Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-ManagementConsole" -ForegroundColor Yellow
} else {
    Write-Success "IIS è installato sul sistema."
}

# Suggerimenti finali
Write-StepHeader "Configurazione completata"
Write-Host "L'ambiente è stato configurato per l'esecuzione di Email Service API." -ForegroundColor Cyan
Write-Host "Riepilogo configurazione:" -ForegroundColor Cyan
Write-Host "- Ambiente: $Environment"
Write-Host "- Server SMTP: $SmtpServer:$SmtpPort"
Write-Host "- Account email: $Username"
Write-Host "- Nome mittente: $SenderName"
Write-Host "- Porta API: $ApiPort (aperta nel firewall)"

Write-Host "`nPer installare il servizio, esegui:" -ForegroundColor Cyan
Write-Host ".\Install-EmailService.ps1" -ForegroundColor Yellow