#!/bin/bash
# test.sh - Build and run the offline test suite (OniAccess.Tests) on the Mac.
# The tests are a .NET Framework 4.8 program, so they run under Mono
# (brew install mono). windows/test.ps1 is the Windows counterpart.
set -euo pipefail

REPO="$(cd "$(dirname "$0")" && pwd)"
source "$REPO/scripts/oni-env.sh"

if ! command -v mono >/dev/null; then
	echo "ERROR: mono not found. Install it with: brew install mono" >&2
	exit 1
fi

TEST_PROJECT="$REPO/OniAccess.Tests/OniAccess.Tests.csproj"
TEST_EXE="$REPO/OniAccess.Tests/bin/Debug/net48/OniAccess.Tests.exe"

echo "Building tests..."
dotnet build "$TEST_PROJECT" -c Debug

# Mono resolves the Unity types in the test program's fields before Main runs,
# too early for the AssemblyResolve hook Main installs, so the game assemblies
# go on MONO_PATH. The game's copy of the class library (mscorlib, System.*)
# must stay off it, since it does not match the installed runtime; the
# directory therefore links only the game and Unity assemblies.
MONO_DIR="$REPO/OniAccess.Tests/bin/mono-path"
rm -rf "$MONO_DIR"
mkdir -p "$MONO_DIR"
for dll in "$ONI_MANAGED"/*.dll; do
	case "$(basename "$dll")" in
		mscorlib.dll|System.dll|System.*.dll|netstandard.dll|Mono.*.dll|Microsoft.*.dll) ;;
		*) ln -s "$dll" "$MONO_DIR/" ;;
	esac
done

echo
echo "Running tests..."
MONO_PATH="$MONO_DIR" mono "$TEST_EXE"
