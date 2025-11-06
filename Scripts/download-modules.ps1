# Download PowerShell modules for bundling
# This script downloads only the specific modules needed, avoiding the full meta-modules
$ErrorActionPreference = 'Stop'

Write-Host "Downloading PowerShell modules for bundling..."

# Resolve repository root and Modules directory regardless of where the script is invoked from
$repoRoot = (Split-Path $PSScriptRoot -Parent)
$modulesDir = Join-Path $repoRoot 'Modules'

if (!(Test-Path $modulesDir)) {
    New-Item -Path $modulesDir -ItemType Directory -Force | Out-Null
}

# MicrosoftTeams module (single module)
Write-Host "Downloading MicrosoftTeams module..."
try {
    $nupkg = Join-Path $modulesDir 'MicrosoftTeams.nupkg'
    $zip = Join-Path $modulesDir 'MicrosoftTeams.zip'
    $destination = Join-Path $modulesDir 'MicrosoftTeams'

    Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/MicrosoftTeams" -OutFile $nupkg
    Write-Host "MicrosoftTeams downloaded successfully"
    
    # Extract the module
    Copy-Item $nupkg $zip -Force
    Expand-Archive -Path $zip -DestinationPath $destination -Force
    Remove-Item $zip -Force
    Remove-Item $nupkg -Force
    
    Write-Host "MicrosoftTeams extracted successfully"
} catch {
    Write-Host "Failed to download MicrosoftTeams: $($_.Exception.Message)"
    exit 1
}

# Essential Microsoft.Graph modules (download specific modules, not the full meta-module)
# This is more efficient than downloading Microsoft.Graph which includes everything
$coreGraphModules = @(
    "Microsoft.Graph.Authentication",
    "Microsoft.Graph.Core",
    "Microsoft.Graph.Users",
    "Microsoft.Graph.Users.Actions",
    "Microsoft.Graph.Groups",
    "Microsoft.Graph.Identity.DirectoryManagement"
)

Write-Host "Downloading essential Microsoft.Graph modules..."
foreach ($module in $coreGraphModules) {
    Write-Host "Downloading $module..."
    try {
        $nupkg = Join-Path $modulesDir "$module.nupkg"
        $zip = Join-Path $modulesDir "$module.zip"
        $destination = Join-Path $modulesDir $module

        Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/$module" -OutFile $nupkg
        Write-Host "$module downloaded successfully"
        
        # Extract the module
        Copy-Item $nupkg $zip -Force
        Expand-Archive -Path $zip -DestinationPath $destination -Force
        Remove-Item $zip -Force
        Remove-Item $nupkg -Force
        
        Write-Host "$module extracted successfully"
    } catch {
        Write-Host "Failed to download $module`: $($_.Exception.Message)"
        exit 1
    }
}

Write-Host "All modules downloaded and extracted successfully to $modulesDir"


