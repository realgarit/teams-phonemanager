#!/usr/bin/env bash
#
# Import a signing .p12 into a DEDICATED keychain set up for non-interactive
# codesign. Idempotent. Used both locally and in CI (same flow ⇒ same identity
# ⇒ same designated requirement). Same pattern as tertius.
#
# Usage:  P12_PASSWORD=... Scripts/import-cert.sh <path-to.p12>
# Env:    P12_PASSWORD (required)
#         KEYCHAIN          (default teams-phonemanager-signing.keychain-db)
#         KEYCHAIN_PASSWORD (default teams-phonemanager-build)
#
set -euo pipefail

P12="${1:?usage: import-cert.sh <path-to.p12>}"
: "${P12_PASSWORD:?set P12_PASSWORD}"
KC="${KEYCHAIN:-teams-phonemanager-signing.keychain-db}"
KCPASS="${KEYCHAIN_PASSWORD:-teams-phonemanager-build}"

security create-keychain -p "$KCPASS" "$KC" 2>/dev/null || true
security set-keychain-settings "$KC"                 # no auto-lock timeout
security unlock-keychain -p "$KCPASS" "$KC"
security import "$P12" -k "$KC" -P "$P12_PASSWORD" -T /usr/bin/codesign -T /usr/bin/security
# Allow codesign to use the key without a GUI prompt.
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "$KCPASS" "$KC" >/dev/null 2>&1 || true
# Add to the user search list, preserving existing keychains.
security list-keychains -d user -s "$KC" $(security list-keychains -d user | sed 's/[\"" ]//g')

echo "Imported identity into keychain: $KC"
