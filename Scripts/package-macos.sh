#!/usr/bin/env bash
# Assemble an ad-hoc-signed "Teams Phone Manager.app" from a dotnet publish output.
#
# Usage: ./Scripts/package-macos.sh <publish-dir> <version> <output-dir>
#   e.g. ./Scripts/package-macos.sh ./publish/osx-arm64 3.17.0 ./artifacts
#
# Produces: <output-dir>/Teams Phone Manager.app  (signed, ready to ditto-zip)

set -euo pipefail

PUBLISH_DIR="${1:?publish dir required}"
VERSION="${2:?version required}"
OUTPUT_DIR="${3:?output dir required}"

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
APP_NAME="Teams Phone Manager"
BUNDLE_ID="ch.realgar.teams-phonemanager"
APP="$OUTPUT_DIR/$APP_NAME.app"

rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"

cp -R "$PUBLISH_DIR"/. "$APP/Contents/MacOS/"
chmod +x "$APP/Contents/MacOS/teams-phonemanager"
cp "$REPO_ROOT/assets/icon.icns" "$APP/Contents/Resources/icon.icns"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundleExecutable</key>
    <string>teams-phonemanager</string>
    <key>CFBundleIconFile</key>
    <string>icon.icns</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2026 Realgar. MIT License.</string>
</dict>
</plist>
PLIST

# Ad-hoc signing, no paid Developer ID certificate. Homebrew downloads are not
# quarantined, so signed Mach-Os are enough for a double-click launch via the
# cask. dotnet publish already ad-hoc signs its output; sign only stragglers.
# The bundle itself is NOT sealed: codesign rejects the .NET payload layout
# (managed satellite dlls under Contents/MacOS read as unsigned "nested code"),
# and re-signing the bundle's main executable would trigger that seal attempt.
find "$APP/Contents/MacOS" -type f | while read -r f; do
  if file "$f" | grep -q "Mach-O" && ! codesign --display "$f" >/dev/null 2>&1; then
    codesign --force --sign - "$f"
  fi
done
# Assert every Mach-O carries a signature.
find "$APP/Contents/MacOS" -type f | while read -r f; do
  if file "$f" | grep -q "Mach-O"; then
    codesign --display "$f" >/dev/null 2>&1 || { echo "UNSIGNED: $f" >&2; exit 1; }
  fi
done

echo "Packaged: $APP"
