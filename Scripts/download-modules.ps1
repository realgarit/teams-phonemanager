# Download PowerShell modules for bundling
$ErrorActionPreference = 'Stop'

Write-Host "Downloading MicrosoftTeams and Microsoft.Graph modules..."

# Resolve repository root and Modules directory regardless of where the script is invoked from
$repoRoot = (Split-Path $PSScriptRoot -Parent)
$modulesDir = Join-Path $repoRoot 'Modules'

if (!(Test-Path $modulesDir)) {
    New-Item -Path $modulesDir -ItemType Directory -Force | Out-Null
}

Write-Host "Downloading MicrosoftTeams module..."
try {
    $teamsNupkg = Join-Path $modulesDir 'MicrosoftTeams.nupkg'
    Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/MicrosoftTeams" -OutFile $teamsNupkg
    Write-Host "MicrosoftTeams module downloaded successfully"
} catch {
    Write-Host "Failed to download MicrosoftTeams module: $($_.Exception.Message)"
}

Write-Host "Downloading Microsoft.Graph module..."
try {
    $graphNupkg = Join-Path $modulesDir 'Microsoft.Graph.nupkg'
    Invoke-WebRequest -Uri "https://www.powershellgallery.com/api/v2/package/Microsoft.Graph" -OutFile $graphNupkg
    Write-Host "Microsoft.Graph module downloaded successfully"
} catch {
    Write-Host "Failed to download Microsoft.Graph module: $($_.Exception.Message)"
}

Write-Host "Extracting modules..."
try {
    # Extract MicrosoftTeams
    $teamsZip = Join-Path $modulesDir 'MicrosoftTeams.zip'
    $teamsDest = Join-Path $modulesDir 'MicrosoftTeams'
    Rename-Item $teamsNupkg $teamsZip -Force
    Expand-Archive -Path $teamsZip -DestinationPath $teamsDest -Force
    Remove-Item $teamsZip -Force
    
    # Extract Microsoft.Graph
    $graphZip = Join-Path $modulesDir 'Microsoft.Graph.zip'
    $graphDest = Join-Path $modulesDir 'Microsoft.Graph'
    Rename-Item $graphNupkg $graphZip -Force
    Expand-Archive -Path $graphZip -DestinationPath $graphDest -Force
    Remove-Item $graphZip -Force
    
    Write-Host "Modules extracted successfully"
} catch {
    Write-Host "Failed to extract modules: $($_.Exception.Message)"
}


