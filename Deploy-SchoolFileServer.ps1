# School File Server - IIS Deployment Script
# This script automates the deployment process for Windows Server IIS

param(
    [Parameter(Mandatory=$false)]
    [string]$SiteName = "Default Web Site",
    
    [Parameter(Mandatory=$false)]
    [string]$ApplicationName = "SchoolFileServer",
    
    [Parameter(Mandatory=$false)]
    [string]$DeployPath = "C:\inetpub\wwwroot\SchoolFileServer",
    
    [Parameter(Mandatory=$false)]
    [string]$SourcePath = ".",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipPublish
)

Write-Host "School File Server - IIS Deployment Script" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Check if running as Administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Import IIS module
try {
    Import-Module WebAdministration -ErrorAction Stop
    Write-Host "✓ IIS PowerShell module loaded" -ForegroundColor Green
} catch {
    Write-Host "ERROR: IIS is not installed or WebAdministration module is not available" -ForegroundColor Red
    Write-Host "Please install IIS with ASP.NET Core support first" -ForegroundColor Yellow
    exit 1
}

# Step 1: Publish the application (if not skipped)
if (-not $SkipPublish) {
    Write-Host "`nStep 1: Publishing application..." -ForegroundColor Cyan
    
    if (Test-Path $SourcePath) {
        Push-Location $SourcePath
        
        try {
            Write-Host "Building and publishing application..." -ForegroundColor Yellow
            dotnet publish -c Release -o $DeployPath --self-contained false
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Application published successfully" -ForegroundColor Green
            } else {
                Write-Host "ERROR: Failed to publish application" -ForegroundColor Red
                exit 1
            }
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "ERROR: Source path '$SourcePath' not found" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "`nStep 1: Skipping application publish (as requested)" -ForegroundColor Yellow
}

# Step 2: Create Application Pool
Write-Host "`nStep 2: Configuring IIS Application Pool..." -ForegroundColor Cyan

$appPoolName = $ApplicationName
if (Get-IISAppPool -Name $appPoolName -ErrorAction SilentlyContinue) {
    Write-Host "Application pool '$appPoolName' already exists, updating configuration..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $appPoolName
}

Write-Host "Creating application pool '$appPoolName'..." -ForegroundColor Yellow
New-WebAppPool -Name $appPoolName
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "enable32BitAppOnWin64" -Value $false
Set-ItemProperty -Path "IIS:\AppPools\$appPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"

Write-Host "✓ Application pool '$appPoolName' created and configured" -ForegroundColor Green

# Step 3: Create/Update IIS Application
Write-Host "`nStep 3: Configuring IIS Application..." -ForegroundColor Cyan

$app = Get-WebApplication -Site $SiteName -Name $ApplicationName -ErrorAction SilentlyContinue
if ($app) {
    Write-Host "Application '$ApplicationName' already exists, updating..." -ForegroundColor Yellow
    Remove-WebApplication -Site $SiteName -Name $ApplicationName
}

Write-Host "Creating IIS application '$ApplicationName'..." -ForegroundColor Yellow
New-WebApplication -Site $SiteName -Name $ApplicationName -PhysicalPath $DeployPath -ApplicationPool $appPoolName

Write-Host "✓ IIS application '$ApplicationName' created" -ForegroundColor Green

# Step 4: Set Permissions
Write-Host "`nStep 4: Setting file permissions..." -ForegroundColor Cyan

Write-Host "Setting permissions on application directory..." -ForegroundColor Yellow
icacls $DeployPath /grant "IIS_IUSRS:(OI)(CI)RX" /T | Out-Null

# Ensure uploads directory exists and has write permissions
$uploadsPath = Join-Path $DeployPath "wwwroot\uploads"
if (-not (Test-Path $uploadsPath)) {
    Write-Host "Creating uploads directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $uploadsPath -Force | Out-Null
}

Write-Host "Setting write permissions on uploads directory..." -ForegroundColor Yellow
icacls $uploadsPath /grant "IIS_IUSRS:(OI)(CI)F" /T | Out-Null

Write-Host "✓ File permissions configured" -ForegroundColor Green

# Step 5: Start Application Pool
Write-Host "`nStep 5: Starting application pool..." -ForegroundColor Cyan
Start-WebAppPool -Name $appPoolName
Write-Host "✓ Application pool started" -ForegroundColor Green

# Step 6: Test the deployment
Write-Host "`nStep 6: Testing deployment..." -ForegroundColor Cyan

$testUrl = "http://localhost/$ApplicationName"
Write-Host "Testing URL: $testUrl" -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri $testUrl -TimeoutSec 30 -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Application is responding successfully!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Application responded with status code: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Could not test application (this might be normal if the server is not accessible via localhost)" -ForegroundColor Yellow
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Gray
}

# Step 7: Display completion information
Write-Host "`n" + "="*50 -ForegroundColor Green
Write-Host "DEPLOYMENT COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "="*50 -ForegroundColor Green

Write-Host "`nApplication Details:" -ForegroundColor Cyan
Write-Host "Site Name: $SiteName" -ForegroundColor White
Write-Host "Application Name: $ApplicationName" -ForegroundColor White
Write-Host "Application Pool: $appPoolName" -ForegroundColor White
Write-Host "Physical Path: $DeployPath" -ForegroundColor White
Write-Host "Application URL: $testUrl" -ForegroundColor White

Write-Host "`nDefault Admin Credentials:" -ForegroundColor Cyan
Write-Host "Username: admin (configurable in appsettings.json)" -ForegroundColor White
Write-Host "Password: admin123 (configurable in appsettings.json)" -ForegroundColor White
Write-Host "(Please change these credentials immediately after first login!)" -ForegroundColor Yellow

Write-Host "`nImportant Notes:" -ForegroundColor Cyan
Write-Host "• The SQLite database will be created automatically on first run" -ForegroundColor White
Write-Host "• Default admin credentials can be changed in appsettings.json" -ForegroundColor White
Write-Host "• Ensure ASP.NET Core Hosting Bundle is installed on the server" -ForegroundColor White
Write-Host "• Check IIS logs if the application doesn't start: $DeployPath\logs\" -ForegroundColor White
Write-Host "• For troubleshooting, see the README.md file" -ForegroundColor White

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Browse to $testUrl" -ForegroundColor White
Write-Host "2. Login with the default admin credentials" -ForegroundColor White
Write-Host "3. Change the admin password" -ForegroundColor White
Write-Host "4. Create teacher accounts" -ForegroundColor White
Write-Host "5. Test file upload functionality" -ForegroundColor White

Write-Host "`nDeployment completed at $(Get-Date)" -ForegroundColor Green
