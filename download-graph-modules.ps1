# Download essential Microsoft.Graph modules
$ErrorActionPreference = 'Stop'

Write-Host "Downloading essential Microsoft.Graph modules..."

# Core modules needed for basic Graph functionality
$coreModules = @(
    "Microsoft.Graph.Authentication",
    "Microsoft.Graph.Core", 
    "Microsoft.Graph.Users",
    "Microsoft.Graph.Groups",
    "Microsoft.Graph.Identity.DirectoryManagement"
)

foreach ($module in $coreModules) {
    Write-Host "Downloading $module..."
    try {
        Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/$module" -OutFile "Modules\$module.nupkg"
        Write-Host "$module downloaded successfully"
        
        # Extract the module
        Copy-Item "Modules\$module.nupkg" "Modules\$module.zip"
        Expand-Archive -Path "Modules\$module.zip" -DestinationPath "Modules\$module" -Force
        Remove-Item "Modules\$module.zip"
        Remove-Item "Modules\$module.nupkg"
        
        Write-Host "$module extracted successfully"
    } catch {
        Write-Host "Failed to download $module`: $($_.Exception.Message)"
    }
}

Write-Host "Essential Microsoft.Graph modules download completed"
