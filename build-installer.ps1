# Multi.AI Production Windows Installer Build Script
# Inspired by Rectify11 Installer Architecture

param (
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host " Building Multi.AI Standalone Windows Installer  " -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

$RootDir = $PSScriptRoot
$TempStaging = Join-Path $RootDir "temp_staging"
$TempUninstaller = Join-Path $RootDir "temp_uninstaller"
$PayloadZip = Join-Path $RootDir "MultiAI.Installer\Resources\payload.zip"
$FinalOutput = Join-Path $RootDir "bin\InstallerPackage"

# 1. Clean previous build artifacts
if (Test-Path $TempStaging) { Remove-Item -Recurse -Force $TempStaging }
if (Test-Path $TempUninstaller) { Remove-Item -Recurse -Force $TempUninstaller }
if (Test-Path $PayloadZip) { Remove-Item -Force $PayloadZip }
if (Test-Path $FinalOutput) { Remove-Item -Recurse -Force $FinalOutput }

New-Item -ItemType Directory -Force $TempStaging | Out-Null
New-Item -ItemType Directory -Force $TempUninstaller | Out-Null
New-Item -ItemType Directory -Force (Join-Path $RootDir "MultiAI.Installer\Resources") | Out-Null
New-Item -ItemType Directory -Force $FinalOutput | Out-Null

# 2. Publish Multi.AI unpackaged self-contained
Write-Host "`n[1/4] Publishing Multi.AI standalone binaries..." -ForegroundColor Yellow
& dotnet publish "$RootDir\MultiAI\MultiAI.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:Platform=$Platform `
    -o $TempStaging

if ($LASTEXITCODE -ne 0) { throw "Multi.AI build failed." }

# 3. Publish Uninstaller
Write-Host "`n[2/4] Publishing Uninstaller (Uninstall.exe)..." -ForegroundColor Yellow
& dotnet publish "$RootDir\MultiAI.Uninstaller\MultiAI.Uninstaller.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:Platform=$Platform `
    -o $TempUninstaller

if ($LASTEXITCODE -ne 0) { throw "Uninstaller build failed." }

# Copy Uninstall.exe into staging directory
Copy-Item -Force "$TempUninstaller\Uninstall.exe" "$TempStaging\Uninstall.exe"

# 4. Create payload.zip archive
Write-Host "`n[3/4] Compressing installation payload..." -ForegroundColor Yellow
Compress-Archive -Path "$TempStaging\*" -DestinationPath $PayloadZip -CompressionLevel Optimal

# 5. Build and Publish Multi.AI-Setup.exe
Write-Host "`n[4/4] Publishing single-file Installer (Multi.AI-Setup.exe)..." -ForegroundColor Yellow
& dotnet publish "$RootDir\MultiAI.Installer\MultiAI.Installer.csproj" `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:Platform=$Platform `
    -o $FinalOutput

if ($LASTEXITCODE -ne 0) { throw "Installer build failed." }

# Cleanup temporary staging directories
Remove-Item -Recurse -Force $TempStaging
Remove-Item -Recurse -Force $TempUninstaller

Write-Host "`n=================================================" -ForegroundColor Green
Write-Host " Production Installer Build Succeeded!           " -ForegroundColor Green
Write-Host " Installer File: $FinalOutput\Multi.AI-Setup.exe" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
