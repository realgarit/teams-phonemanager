# Download PowerShell modules for bundling
$ErrorActionPreference = 'Stop'

Write-Host "Downloading MicrosoftTeams module..."
try {
    Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/MicrosoftTeams" -OutFile "Modules\MicrosoftTeams.nupkg"
    Write-Host "MicrosoftTeams module downloaded successfully"
} catch {
    Write-Host "Failed to download MicrosoftTeams module: $($_.Exception.Message)"
}

Write-Host "Downloading Microsoft.Graph module..."
try {
    Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/Microsoft.Graph" -OutFile "Modules\Microsoft.Graph.nupkg"
    Write-Host "Microsoft.Graph module downloaded successfully"
} catch {
    Write-Host "Failed to download Microsoft.Graph module: $($_.Exception.Message)"
}

Write-Host "Extracting modules..."
try {
    # Extract MicrosoftTeams
    Rename-Item "Modules\MicrosoftTeams.nupkg" "Modules\MicrosoftTeams.zip"
    Expand-Archive -Path "Modules\MicrosoftTeams.zip" -DestinationPath "Modules\MicrosoftTeams" -Force
    Remove-Item "Modules\MicrosoftTeams.zip"
    
    # Extract Microsoft.Graph
    Rename-Item "Modules\Microsoft.Graph.nupkg" "Modules\Microsoft.Graph.zip"
    Expand-Archive -Path "Modules\Microsoft.Graph.zip" -DestinationPath "Modules\Microsoft.Graph" -Force
    Remove-Item "Modules\Microsoft.Graph.zip"
    
    Write-Host "Modules extracted successfully"
} catch {
    Write-Host "Failed to extract modules: $($_.Exception.Message)"
}
