param(
    [Parameter(Mandatory=$true)]
    [string]$Username,
    
    [Parameter(Mandatory=$true)]
    [string]$Password,
    
    [string]$SenderName = "Email Service",
    
    [string]$SmtpServer = "smtp.gmail.com",
    
    [int]$SmtpPort = 587,
    
    [bool]$UseSsl = $true
)

# Chiave di registro dove salvare le impostazioni
$registryPath = "SOFTWARE\YourCompany\EmailService"

# Verifica i privilegi di amministratore
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Questo script deve essere eseguito come amministratore"
    exit 1
}

try {
    # Crea la chiave se non esiste
    if (-not (Test-Path "HKLM:\$registryPath")) {
        New-Item -Path "HKLM:\$registryPath" -Force | Out-Null
    }
    
    # Imposta i valori nel registro
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "Username" -Value $Username
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "Password" -Value $Password
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SenderName" -Value $SenderName
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SenderEmail" -Value $Username
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SmtpServer" -Value $SmtpServer
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "SmtpPort" -Value $SmtpPort
    Set-ItemProperty -Path "HKLM:\$registryPath" -Name "UseSsl" -Value $([int]$UseSsl)
    
    # Restringe l'accesso alla chiave ai soli amministratori e SYSTEM
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
    
    $acl.AddAccessRule($adminRule)
    $acl.AddAccessRule($systemRule)
    
    # Se l'applicazione gira sotto un account di servizio specifico, aggiungi anche quello
    # Per esempio, se usi un account di servizio "NetworkService":
    $serviceRule = New-Object System.Security.AccessControl.RegistryAccessRule(
        "NetworkService",
        "ReadKey",
        "ContainerInherit,ObjectInherit",
        "None",
        "Allow"
    )
    $acl.AddAccessRule($serviceRule)
    
    Set-Acl "HKLM:\$registryPath" $acl
    
    Write-Host "Configurazione completata con successo" -ForegroundColor Green
}
catch {
    Write-Error "Errore durante la configurazione: $_"
    exit 1
}