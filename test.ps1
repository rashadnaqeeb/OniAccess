# test.ps1 - Build and run offline tests for OniAccess.
# Tests the handler stack contracts without requiring the game.

$ErrorActionPreference = "Stop"

# Locate the game's Managed directory for building against game assemblies.
# Checks ONI_MANAGED env var first, then auto-detects from Steam's library folders.
if (-not $env:ONI_MANAGED) {
    $SteamPaths = @()
    $RegSteam = (Get-ItemProperty -Path "HKLM:\SOFTWARE\WOW6432Node\Valve\Steam" -Name InstallPath -ErrorAction SilentlyContinue).InstallPath
    $DefaultSteam = if ($RegSteam) { $RegSteam } else { "C:\Program Files (x86)\Steam" }
    if (Test-Path "$DefaultSteam\steamapps") {
        $SteamPaths += $DefaultSteam
    }
    $LibFolders = "$DefaultSteam\steamapps\libraryfolders.vdf"
    if (Test-Path $LibFolders) {
        $content = Get-Content $LibFolders -Raw
        [regex]::Matches($content, '"path"\s+"([^"]+)"') | ForEach-Object {
            $p = $_.Groups[1].Value -replace '\\\\', '\'
            if ($p -ne $DefaultSteam -and (Test-Path "$p\steamapps")) {
                $SteamPaths += $p
            }
        }
    }
    foreach ($steam in $SteamPaths) {
        $candidate = "$steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed"
        if (Test-Path $candidate) {
            $env:ONI_MANAGED = $candidate
            break
        }
    }
    if (-not $env:ONI_MANAGED) {
        Write-Host "ERROR: Could not find ONI. Set the ONI_MANAGED environment variable to" -ForegroundColor Red
        Write-Host "  <SteamLibrary>\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed" -ForegroundColor Red
        exit 1
    }
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
