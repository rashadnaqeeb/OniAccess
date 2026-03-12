@echo off
REM enable-mod.cmd - Enables the OniAccess mod in Oxygen Not Included.
REM Double-click this file or press Enter on it to run.
REM Requires that the game has been launched at least once after subscribing
REM to the mod, so the game has discovered it and added it to mods.json.

powershell -ExecutionPolicy Bypass -Command ^
 $DocsDir = [Environment]::GetFolderPath('MyDocuments'); ^
 $ModsJson = Join-Path $DocsDir 'Klei\OxygenNotIncluded\mods\mods.json'; ^
 if (-not (Test-Path $ModsJson)) { ^
   Write-Host 'mods.json not found. Please launch Oxygen Not Included once, then close it and run this script again.' -ForegroundColor Yellow; ^
   Read-Host 'Press Enter to close'; ^
   exit 1; ^
 } ^
 $json = Get-Content $ModsJson -Raw -Encoding UTF8 ^| ConvertFrom-Json; ^
 $found = $false; ^
 foreach ($mod in $json.mods) { ^
   if ($mod.staticID -eq 'OniAccess' -or $mod.label.id -eq 'OniAccess') { ^
     $mod.enabled = $true; ^
     $mod.enabledForDlc = @('', 'EXPANSION1_ID'); ^
     $mod.crash_count = 0; ^
     $mod.status = 1; ^
     $found = $true; ^
     break; ^
   } ^
 } ^
 if (-not $found) { ^
   Write-Host 'OniAccess mod not found in mods.json.' -ForegroundColor Red; ^
   Write-Host 'Please subscribe to the mod on the Steam Workshop, launch the game once, then close it and run this script again.' -ForegroundColor Yellow; ^
   Read-Host 'Press Enter to close'; ^
   exit 1; ^
 } ^
 $jsonText = $json ^| ConvertTo-Json -Depth 4; ^
 [System.IO.File]::WriteAllText($ModsJson, $jsonText, [System.Text.UTF8Encoding]::new($false)); ^
 Write-Host 'OniAccess mod enabled. Launch the game to play.' -ForegroundColor Green; ^
 Read-Host 'Press Enter to close'
