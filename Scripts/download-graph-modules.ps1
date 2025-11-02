# Download essential Microsoft.Graph modules
$ErrorActionPreference = 'Stop'

Write-Host "Downloading essential Microsoft.Graph modules..."

# Resolve repository root and Modules directory regardless of where the script is invoked from
$repoRoot = (Split-Path $PSScriptRoot -Parent)
$modulesDir = Join-Path $repoRoot 'Modules'

if (!(Test-Path $modulesDir)) {
    New-Item -Path $modulesDir -ItemType Directory -Force | Out-Null
}

# Core modules needed for basic Graph functionality
$coreModules = @(
    "Microsoft.Graph.Authentication",
    "Microsoft.Graph.Core",
    "Microsoft.Graph.Users",
    "Microsoft.Graph.Users.Actions",
    "Microsoft.Graph.Groups",
    "Microsoft.Graph.Identity.DirectoryManagement"
)

foreach ($module in $coreModules) {
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
    }
}

Write-Host "Essential Microsoft.Graph modules download completed"


