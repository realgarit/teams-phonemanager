param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath = ".\publish\framework-dependent",
    [switch]$Clean = $false,
    [switch]$Help = $false
)

$repoRoot = (Split-Path $PSScriptRoot -Parent)

if ($Help) {
    Write-Host "Teams Phone Manager - Publish Script" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage: .\Scripts\publish.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -Configuration <config>  Build configuration (Debug|Release). Default: Release" -ForegroundColor White
    Write-Host "  -Runtime <runtime>       Target runtime (win-x64|win-x86|win-arm64). Default: win-x64" -ForegroundColor White
    Write-Host "  -OutputPath <path>       Output directory. Default: .\\publish\\framework-dependent (repo root)" -ForegroundColor White
    Write-Host "  -Clean                   Clean output directory before publishing" -ForegroundColor White
    Write-Host "  -Help                    Show this help message" -ForegroundColor White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Yellow
    Write-Host "  .\Scripts\publish.ps1                                    # Default publish" -ForegroundColor White
    Write-Host "  .\Scripts\publish.ps1 -Configuration Debug              # Debug build" -ForegroundColor White
    Write-Host "  .\Scripts\publish.ps1 -Runtime win-x86                  # 32-bit build" -ForegroundColor White
    Write-Host "  .\Scripts\publish.ps1 -Clean                            # Clean publish" -ForegroundColor White
    exit 0
}

Write-Host "Teams Phone Manager - Publishing Framework-Dependent Deployment" -ForegroundColor Green
Write-Host "================================================================" -ForegroundColor Green
Write-Host ""

try {
    $dotnetVersion = dotnet --version
    Write-Host "Using .NET SDK version: $dotnetVersion" -ForegroundColor Cyan
} catch {
    Write-Host "Error: .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Normalize OutputPath to be relative to repo root when a relative path is provided
if (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    # Treat as repo-root relative
    $OutputPath = Join-Path $repoRoot $OutputPath
}

if ($Clean -and (Test-Path $OutputPath)) {
    Write-Host "Cleaning output directory: $OutputPath" -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

if (!(Test-Path $OutputPath)) {
    Write-Host "Creating output directory: $OutputPath" -ForegroundColor Yellow
    New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null
}

Write-Host ""
Write-Host "Publishing configuration:" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor White
Write-Host "  Runtime: $Runtime" -ForegroundColor White
Write-Host "  Output Path: $OutputPath" -ForegroundColor White
Write-Host "  Self-Contained: false (requires .NET 8 Runtime)" -ForegroundColor White
Write-Host ""

Write-Host "Starting publish process..." -ForegroundColor Yellow

try {
    $publishCommand = @(
        "publish"
        "--configuration", $Configuration
        "--runtime", $Runtime
        "--output", $OutputPath
        "--self-contained", "false"
        "--verbosity", "minimal"
    )

    Write-Host "Executing: dotnet $($publishCommand -join ' ')" -ForegroundColor Gray
    dotnet $publishCommand

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Publish completed successfully!" -ForegroundColor Green
        Write-Host ""
        
        $exePath = Join-Path $OutputPath "teams-phonemanager.exe"
        if (Test-Path $exePath) {
            $fileSize = (Get-Item $exePath).Length
            $fileSizeMB = [math]::Round($fileSize / 1MB, 2)
            Write-Host "📁 Output directory: $OutputPath" -ForegroundColor Cyan
            Write-Host "📄 Executable: teams-phonemanager.exe" -ForegroundColor Cyan
            Write-Host "📊 Executable size: $fileSizeMB MB" -ForegroundColor Cyan
            
            $fileCount = (Get-ChildItem -Path $OutputPath -File).Count
            Write-Host "📦 Total files: $fileCount" -ForegroundColor Cyan
        }
        
        Write-Host ""
        Write-Host "📋 Prerequisites for target machines:" -ForegroundColor Yellow
        Write-Host "   • .NET 8 Runtime (Desktop)" -ForegroundColor White
        Write-Host "   • Windows 10/11 or Windows Server 2019+" -ForegroundColor White
        Write-Host ""
        Write-Host "🔗 Download .NET 8 Runtime:" -ForegroundColor Yellow
        Write-Host "   https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Blue
        Write-Host ""
        Write-Host "💡 To run the application:" -ForegroundColor Yellow
        Write-Host "   1. Ensure .NET 8 Runtime is installed" -ForegroundColor White
        Write-Host "   2. Copy the entire '$OutputPath' folder to target machine" -ForegroundColor White
        Write-Host "   3. Run 'teams-phonemanager.exe'" -ForegroundColor White
        
    } else {
        Write-Host "❌ Publish failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    
} catch {
    Write-Host "❌ Error during publish: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}


