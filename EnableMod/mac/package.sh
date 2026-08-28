#!/bin/bash
# package.sh - Compile EnableMod.js into EnableMod.app, sign and notarize it,
# and zip it for distribution. Produces EnableMod.app (ignored by git) and
# EnableMod.app.zip (committed; the getting-started guide links to it) in the
# repo root.
#
# With the Developer ID certificate in the keychain and a notarytool keychain
# profile named "notary" (xcrun notarytool store-credentials notary ...), the
# app is signed with the hardened runtime, notarized, and stapled, so it opens
# on any Mac without Gatekeeper prompts. Pass --no-notarize to skip the upload
# (local testing only; do not commit a zip built that way). Without the
# certificate it falls back to an ad-hoc signature with a warning.
set -euo pipefail

DIR="$(cd "$(dirname "$0")" && pwd)"
REPO="$(cd "$DIR/../.." && pwd)"
APP="$REPO/EnableMod.app"
ZIP="$REPO/EnableMod.app.zip"
PLIST="$APP/Contents/Info.plist"
IDENTITY="Developer ID Application: Rashad Naqeeb (D5L87433H8)"
NOTARY_PROFILE="notary"

NOTARIZE=1
[ "${1:-}" = "--no-notarize" ] && NOTARIZE=0

make_zip() {
	rm -f "$ZIP"
	ditto -c -k --sequesterRsrc --keepParent "$APP" "$ZIP"
}

rm -rf "$APP" "$ZIP"
osacompile -l JavaScript -o "$APP" "$DIR/EnableMod.js"

/usr/libexec/PlistBuddy -c "Add :CFBundleIdentifier string com.oniaccess.enablemod" "$PLIST"

if ! security find-identity -v -p codesigning | grep -q "$IDENTITY"; then
	echo "warning: '$IDENTITY' not in keychain; ad-hoc signing (Gatekeeper will block this build)" >&2
	codesign --force --sign - "$APP"
	make_zip
	echo "Built $ZIP (ad-hoc)"
	exit 0
fi

codesign --force --options runtime --timestamp \
	--entitlements "$DIR/entitlements.plist" --sign "$IDENTITY" "$APP"
make_zip

if [ "$NOTARIZE" = 1 ]; then
	echo "Submitting for notarization..."
	RESULT="$(xcrun notarytool submit "$ZIP" --keychain-profile "$NOTARY_PROFILE" --wait 2>&1)" || true
	echo "$RESULT"
	if ! grep -q "status: Accepted" <<<"$RESULT"; then
		ID="$(awk '/^  id:/ { print $2; exit }' <<<"$RESULT")"
		[ -n "$ID" ] && xcrun notarytool log "$ID" --keychain-profile "$NOTARY_PROFILE" || true
		echo "error: notarization failed" >&2
		exit 1
	fi
	# Stapling writes the ticket into the bundle, so the zip must be rebuilt.
	xcrun stapler staple "$APP"
	make_zip
	spctl -a -vv "$APP"
	echo "Built $ZIP (notarized)"
else
	echo "Built $ZIP (signed, not notarized)"
fi
