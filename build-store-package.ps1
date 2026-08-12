# MultiAI Microsoft Store Packaging Automation Script

param (
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host " Building MultiAI for Microsoft Store Submission " -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

$OutputDir = Join-Path $PSScriptRoot "MultiAI\bin\StorePackages"

Write-Host "Publishing MSIX Store Upload package..." -ForegroundColor Yellow

& dotnet publish "$PSScriptRoot\MultiAI\MultiAI.csproj" `
    -c $Configuration `
    -p:Platform=$Platform `
    -p:GenerateAppInstallerFile=False `
    -p:AppxPackageSigningEnabled=false `
    -p:AppxPackageDir="$OutputDir\" `
    -p:AppxBundle=Always `
    -p:UapAppxPackageBuildMode=StoreUpload `
    -p:AppxSymbolPackageEnabled=false

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n=================================================" -ForegroundColor Green
    Write-Host " Store Packaging Completed Successfully! " -ForegroundColor Green
    Write-Host " Output Directory: $OutputDir" -ForegroundColor Green
    Write-Host " Look for the .msixupload file in the directory above." -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
} else {
    Write-Host "`nFailed to build Store MSIX package!" -ForegroundColor Red
}
