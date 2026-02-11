#!/bin/bash
# Download PowerShell modules for bundling
# This script downloads only the specific modules needed, avoiding the full meta-modules

set -e

echo "Downloading PowerShell modules for bundling..."

# Resolve repository root and Modules directory regardless of where the script is invoked from
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
MODULES_DIR="$REPO_ROOT/Modules"

# Create Modules directory if it doesn't exist
mkdir -p "$MODULES_DIR"

# MicrosoftTeams module (single module)
echo "Downloading MicrosoftTeams module..."
TEAMS_NUPKG="$MODULES_DIR/MicrosoftTeams.nupkg"
TEAMS_ZIP="$MODULES_DIR/MicrosoftTeams.zip"
TEAMS_DEST="$MODULES_DIR/MicrosoftTeams"

curl -L -o "$TEAMS_NUPKG" "https://www.powershellgallery.com/api/v2/package/MicrosoftTeams"
echo "MicrosoftTeams downloaded successfully"

# Extract the module
cp "$TEAMS_NUPKG" "$TEAMS_ZIP"
unzip -q -o "$TEAMS_ZIP" -d "$TEAMS_DEST"
rm -f "$TEAMS_ZIP"
rm -f "$TEAMS_NUPKG"
echo "MicrosoftTeams extracted successfully"

# Essential Microsoft.Graph modules (download specific modules, not the full meta-module)
# This is more efficient than downloading Microsoft.Graph which includes everything
CORE_GRAPH_MODULES=(
    "Microsoft.Graph.Authentication"
    "Microsoft.Graph.Users"
    "Microsoft.Graph.Users.Actions"
    "Microsoft.Graph.Groups"
    "Microsoft.Graph.Identity.DirectoryManagement"
)

echo "Downloading essential Microsoft.Graph modules..."
for module in "${CORE_GRAPH_MODULES[@]}"; do
    echo "Downloading $module..."
    NUPKG="$MODULES_DIR/${module}.nupkg"
    ZIP="$MODULES_DIR/${module}.zip"
    DEST="$MODULES_DIR/$module"
    
    curl -L -o "$NUPKG" "https://www.powershellgallery.com/api/v2/package/$module"
    echo "$module downloaded successfully"
    
    # Extract the module
    cp "$NUPKG" "$ZIP"
    unzip -q -o "$ZIP" -d "$DEST"
    rm -f "$ZIP"
    rm -f "$NUPKG"
    
    echo "$module extracted successfully"
done

echo "All modules downloaded and extracted successfully to $MODULES_DIR"

