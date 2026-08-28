#!/bin/bash
# build.sh - Build and deploy OniAccess to the Mac game's local mods directory.
# Mac counterpart of build.ps1: builds the DLL, deploys it, and keeps the mod
# enabled in mods.json (the game disables it after crashes or version changes).
set -euo pipefail

NO_BUILD=0
for arg in "$@"; do
	case "$arg" in
		--no-build) NO_BUILD=1 ;;
		-h|--help)
			echo "Usage: ./build.sh [--no-build]"
			echo "  --no-build  Skip building, just copy the last built DLL and patch mods.json"
			exit 0 ;;
		*) echo "Unknown option: $arg" >&2; exit 1 ;;
	esac
done

REPO="$(cd "$(dirname "$0")" && pwd)"

# Locate the game's Managed directory for building against game assemblies.
# Checks ONI_MANAGED first, then every Steam library folder.
if [ -z "${ONI_MANAGED:-}" ]; then
	STEAM="$HOME/Library/Application Support/Steam"
	LIBRARIES=("$STEAM")
	VDF="$STEAM/steamapps/libraryfolders.vdf"
	if [ -f "$VDF" ]; then
		while IFS= read -r path; do
			LIBRARIES+=("$path")
		done < <(sed -n 's/^[[:space:]]*"path"[[:space:]]*"\(.*\)".*/\1/p' "$VDF")
	fi
	for lib in "${LIBRARIES[@]}"; do
		candidate="$lib/steamapps/common/OxygenNotIncluded/OxygenNotIncluded.app/Contents/Resources/Data/Managed"
		if [ -d "$candidate" ]; then
			export ONI_MANAGED="$candidate"
			break
		fi
	done
	if [ -z "${ONI_MANAGED:-}" ]; then
		echo "ERROR: Could not find ONI. Set ONI_MANAGED to" >&2
		echo "  <SteamLibrary>/steamapps/common/OxygenNotIncluded/OxygenNotIncluded.app/Contents/Resources/Data/Managed" >&2
		exit 1
	fi
fi

PROJECT_DIR="$REPO/OniAccess"
BUILD_OUTPUT="$PROJECT_DIR/bin/Release/net48/OniAccess.dll"
DATA_DIR="$HOME/Library/Application Support/unity.Klei.Oxygen Not Included"
MOD_DIR="$DATA_DIR/mods/local/OniAccess"
MODS_JSON="$DATA_DIR/mods/mods.json"

# --- Sync version from .csproj to mod_info.yaml ---
VERSION="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$PROJECT_DIR/OniAccess.csproj")"
sed -i '' "s/version: \".*\"/version: \"$VERSION\"/" "$PROJECT_DIR/mod_info.yaml"

# --- Build ---
if [ "$NO_BUILD" -eq 0 ]; then
	echo "Building OniAccess..."
	dotnet build "$PROJECT_DIR/OniAccess.csproj" -c Release
fi

if [ ! -f "$BUILD_OUTPUT" ]; then
	echo "ERROR: DLL not found at $BUILD_OUTPUT" >&2
	exit 1
fi

# --- Copy DLL, metadata, and the Mac Prism native library ---
mkdir -p "$MOD_DIR/native/osx"
cp "$BUILD_OUTPUT" "$MOD_DIR/OniAccess.dll"
cp "$PROJECT_DIR/mod_info.yaml" "$PROJECT_DIR/mod.yaml" "$MOD_DIR/"
cp "$REPO/prism/native/osx/libprism.dylib" "$MOD_DIR/native/osx/libprism.dylib"
echo "Deployed Prism native library to $MOD_DIR/native/osx"

# --- Copy translation and audio files ---
shopt -s nullglob
PO_FILES=("$REPO"/translations/*.po)
if [ "${#PO_FILES[@]}" -gt 0 ]; then
	mkdir -p "$MOD_DIR/translations"
	cp "${PO_FILES[@]}" "$MOD_DIR/translations/"
	echo "Deployed ${#PO_FILES[@]} translation file(s)"
fi
OGG_FILES=("$REPO"/audio/*.ogg)
if [ "${#OGG_FILES[@]}" -gt 0 ]; then
	mkdir -p "$MOD_DIR/audio"
	cp "${OGG_FILES[@]}" "$MOD_DIR/audio/"
	echo "Deployed ${#OGG_FILES[@]} audio file(s)"
fi
shopt -u nullglob

# --- Patch mods.json ---
# Same logic the EnableMod applet uses: enabled for base game and Spaced Out,
# crash count reset, status Installed.
case "$(osascript -l JavaScript "$REPO/EnableMod/mac/EnableMod.js" --quiet)" in
	OK) echo "Patched mods.json - mod is enabled." ;;
	NOT_FOUND)
		echo "Mod entry not found in mods.json - game will discover it on next launch."
		echo "Then run ./build.sh --no-build (or EnableMod.app) to enable it." ;;
	NO_FILE)
		echo "mods.json not found - game will create it on first launch."
		echo "Then run ./build.sh --no-build (or EnableMod.app) to enable it." ;;
	*) echo "ERROR: unexpected result from EnableMod.js" >&2; exit 1 ;;
esac

echo
echo "Done. Launch the game."
