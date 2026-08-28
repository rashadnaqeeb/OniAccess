#!/bin/bash
# package.sh - Compile EnableMod.js into EnableMod.app and zip it for distribution.
# Produces EnableMod.app (ignored by git) and EnableMod.app.zip (committed; the
# getting-started guide links to it) in the repo root.
set -euo pipefail

DIR="$(cd "$(dirname "$0")" && pwd)"
REPO="$(cd "$DIR/../.." && pwd)"
APP="$REPO/EnableMod.app"
ZIP="$REPO/EnableMod.app.zip"
PLIST="$APP/Contents/Info.plist"

rm -rf "$APP" "$ZIP"
osacompile -l JavaScript -o "$APP" "$DIR/EnableMod.js"

/usr/libexec/PlistBuddy -c "Add :CFBundleIdentifier string com.oniaccess.enablemod" "$PLIST"

# Ad-hoc signature so the bundle's seal matches its contents. Gatekeeper still
# requires Open Anyway on first launch (no Developer ID), but a valid seal
# avoids the "app is damaged" refusal that a broken one would produce.
codesign --force --deep --sign - "$APP"

ditto -c -k --sequesterRsrc --keepParent "$APP" "$ZIP"
echo "Built $ZIP"
