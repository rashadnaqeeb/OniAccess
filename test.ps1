# test.ps1 - Build and run offline tests for OniAccess.
# Tests the handler stack contracts without requiring the game.

$ErrorActionPreference = "Stop"

if (-not $env:ONI_MANAGED) {
    $env:ONI_MANAGED = "C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed"
}

$TestProject = "$PSScriptRoot\OniAccess.Tests\OniAccess.Tests.csproj"
$TestExe     = "$PSScriptRoot\OniAccess.Tests\bin\Debug\net472\OniAccess.Tests.exe"

Write-Host "Building tests..." -ForegroundColor Cyan
dotnet build $TestProject -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Running tests..." -ForegroundColor Cyan
& $TestExe
exit $LASTEXITCODE
