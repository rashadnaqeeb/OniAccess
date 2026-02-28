# build.ps1 - Build and deploy OniAccess mod to ONI's local mods directory.
# Also patches mods.json to ensure the mod stays enabled (prevents the game
# from disabling it after crashes or version mismatches).

param(
    [switch]$NoBuild,
    [switch]$Help
)

if ($Help) {
    Write-Host "Usage: .\build.ps1 [-NoBuild] [-Help]"
    Write-Host "  -NoBuild  Skip building, just copy the last built DLL and patch mods.json"
    Write-Host "  -Help     Show this help"
    exit 0
}

$ErrorActionPreference = "Stop"

# Ensure ONI_MANAGED is set for building against game assemblies
if (-not $env:ONI_MANAGED) {
    $env:ONI_MANAGED = "C:\Program Files (x86)\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed"
}

$ProjectDir  = "$PSScriptRoot\OniAccess"
$BuildOutput = "$ProjectDir\bin\Debug\net472\OniAccess.dll"
$ModDir      = "$env:USERPROFILE\Documents\Klei\OxygenNotIncluded\mods\local\OniAccess"
$ModsJson    = "$env:USERPROFILE\Documents\Klei\OxygenNotIncluded\mods\mods.json"

# --- Build ---
if (-not $NoBuild) {
    Write-Host "Building OniAccess..." -ForegroundColor Cyan
    dotnet build "$ProjectDir\OniAccess.csproj" -c Debug
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build FAILED." -ForegroundColor Red
        exit 1
    }
}

if (-not (Test-Path $BuildOutput)) {
    Write-Host "ERROR: DLL not found at $BuildOutput" -ForegroundColor Red
    exit 1
}

# --- Copy DLL and native dependencies ---
if (-not (Test-Path $ModDir)) {
    New-Item -ItemType Directory -Path $ModDir -Force | Out-Null
}
Copy-Item $BuildOutput "$ModDir\OniAccess.dll" -Force

# Tolk and screen reader driver DLLs go in a "native" subfolder so ONI's
# mod loader doesn't try to load them as .NET assemblies (BadImageFormatException).
# SetDllDirectory(native) lets LoadLibrary find them at runtime.
$NativeDir = "$ModDir\native"
if (-not (Test-Path $NativeDir)) {
    New-Item -ItemType Directory -Path $NativeDir -Force | Out-Null
}
$TolkSrc = "$PSScriptRoot\tolk\dist"
$TolkDlls = @("Tolk.dll", "nvdaControllerClient64.dll", "SAAPI64.dll")
foreach ($dll in $TolkDlls) {
    $src = Join-Path $TolkSrc $dll
    if (Test-Path $src) {
        Copy-Item $src "$NativeDir\$dll" -Force
    } else {
        Write-Host "WARNING: $dll not found at $src" -ForegroundColor Yellow
    }
}
Write-Host "Deployed DLL and Tolk libraries to $ModDir" -ForegroundColor Green

# --- Patch mods.json ---
# Ensures the mod entry has enabledForDlc covering both base game ("") and
# Spaced Out ("EXPANSION1_ID"), crash_count reset to 0, enabled = true.
# IMPORTANT: Must write UTF-8 WITHOUT BOM. PowerShell's -Encoding UTF8 adds
# a BOM which corrupts the file for Unity's Mono JSON parser, causing the
# game to silently discard all mod state and re-discover mods as disabled.
if (Test-Path $ModsJson) {
    $json = Get-Content $ModsJson -Raw | ConvertFrom-Json

    $found = $false
    foreach ($mod in $json.mods) {
        if ($mod.label.id -eq "OniAccess") {
            $mod.enabled = $true
            $mod.enabledForDlc = @("", "EXPANSION1_ID")
            $mod.crash_count = 0
            $mod.status = 1  # Status.Installed
            $found = $true
            break
        }
    }

    if (-not $found) {
        Write-Host "Mod entry not found in mods.json - game will discover it on next launch." -ForegroundColor Yellow
        Write-Host "Enable it in the Mods screen, then future deploys will keep it enabled."
    } else {
        $jsonText = $json | ConvertTo-Json -Depth 10
        [System.IO.File]::WriteAllText($ModsJson, $jsonText, [System.Text.UTF8Encoding]::new($false))
        Write-Host "Patched mods.json - mod is enabled." -ForegroundColor Green
    }
} else {
    Write-Host "mods.json not found - game will create it on first launch." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Done. Launch the game." -ForegroundColor Cyan
