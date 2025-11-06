#!/bin/bash
# Teams Phone Manager - Publish Script for Windows (Cross-Compilation)
# Builds and publishes the application for Windows from macOS/Linux

set -e

# Default values
CONFIGURATION="Release"
RUNTIME="win-x64"
OUTPUT_PATH="./publish/windows"
CLEAN=false
HELP=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -Configuration|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -Runtime|--runtime)
            RUNTIME="$2"
            shift 2
            ;;
        -OutputPath|--output-path)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        -Clean|--clean)
            CLEAN=true
            shift
            ;;
        -Help|--help|-h)
            HELP=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use -Help for usage information"
            exit 1
            ;;
    esac
done

# Show help
if [ "$HELP" = true ]; then
    echo -e "\033[32mTeams Phone Manager - Windows Publish Script (Cross-Compilation)\033[0m"
    echo ""
    echo -e "\033[33mUsage: ./Scripts/publish-windows.sh [options]\033[0m"
    echo ""
    echo -e "\033[33mOptions:\033[0m"
    echo "  -Configuration <config>  Build configuration (Debug|Release). Default: Release"
    echo "  -Runtime <runtime>       Target runtime (win-x64|win-x86|win-arm64). Default: win-x64"
    echo "  -OutputPath <path>       Output directory. Default: ./publish/windows (repo root)"
    echo "  -Clean                   Clean output directory before publishing"
    echo "  -Help                    Show this help message"
    echo ""
    echo -e "\033[33mExamples:\033[0m"
    echo "  ./Scripts/publish-windows.sh                                    # Default Windows x64 build"
    echo "  ./Scripts/publish-windows.sh -Configuration Debug              # Debug build"
    echo "  ./Scripts/publish-windows.sh -Runtime win-x86                  # 32-bit Windows build"
    echo "  ./Scripts/publish-windows.sh -Runtime win-arm64                # Windows ARM64 build"
    echo "  ./Scripts/publish-windows.sh -Clean                            # Clean publish"
    exit 0
fi

# Resolve repository root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo -e "\033[32mTeams Phone Manager - Publishing for Windows (Cross-Compilation)\033[0m"
echo "================================================================"
echo ""

# Check for .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "\033[31mError: .NET SDK not found. Please install .NET 8 SDK.\033[0m"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "\033[36mUsing .NET SDK version: $DOTNET_VERSION\033[0m"

# Validate Windows runtime identifier
case "$RUNTIME" in
    win-x64|win-x86|win-arm64)
        echo -e "\033[36mTarget runtime: $RUNTIME\033[0m"
        ;;
    *)
        echo -e "\033[31mError: Invalid Windows runtime identifier: $RUNTIME\033[0m"
        echo "Valid options: win-x64, win-x86, win-arm64"
        exit 1
        ;;
esac

# Normalize output path (make it absolute if relative)
if [[ "$OUTPUT_PATH" != /* ]]; then
    OUTPUT_PATH="$REPO_ROOT/$OUTPUT_PATH"
fi

# Clean output directory if requested
if [ "$CLEAN" = true ] && [ -d "$OUTPUT_PATH" ]; then
    echo -e "\033[33mCleaning output directory: $OUTPUT_PATH\033[0m"
    rm -rf "$OUTPUT_PATH"
fi

# Create output directory if it doesn't exist
if [ ! -d "$OUTPUT_PATH" ]; then
    echo -e "\033[33mCreating output directory: $OUTPUT_PATH\033[0m"
    mkdir -p "$OUTPUT_PATH"
fi

echo ""
echo -e "\033[36mPublishing configuration:\033[0m"
echo "  Configuration: $CONFIGURATION"
echo "  Runtime: $RUNTIME"
echo "  Output Path: $OUTPUT_PATH"
echo "  Self-Contained: false (requires .NET 8 Runtime)"
echo "  Output Type: WinExe (Windows executable)"
echo ""

echo -e "\033[33mStarting publish process...\033[0m"

# Change to repo root for dotnet publish
cd "$REPO_ROOT"

# Build publish command
# Keep OutputType as WinExe for Windows (no override needed)
PUBLISH_CMD=(
    "publish"
    "--configuration" "$CONFIGURATION"
    "--runtime" "$RUNTIME"
    "--output" "$OUTPUT_PATH"
    "--self-contained" "false"
    "--verbosity" "minimal"
)

echo -e "\033[90mExecuting: dotnet ${PUBLISH_CMD[*]}\033[0m"

# Run dotnet publish
if dotnet "${PUBLISH_CMD[@]}"; then
    echo ""
    echo -e "\033[32m✅ Publish completed successfully!\033[0m"
    echo ""
    
    # Find the executable (Windows uses .exe extension)
    EXE_NAME="teams-phonemanager.exe"
    EXE_PATH="$OUTPUT_PATH/$EXE_NAME"
    
    if [ -f "$EXE_PATH" ]; then
        FILE_SIZE=$(stat -f%z "$EXE_PATH" 2>/dev/null || stat -c%s "$EXE_PATH" 2>/dev/null)
        FILE_SIZE_MB=$(echo "scale=2; $FILE_SIZE / 1048576" | bc)
        echo -e "\033[36m📁 Output directory: $OUTPUT_PATH\033[0m"
        echo -e "\033[36m📄 Executable: $EXE_NAME\033[0m"
        echo -e "\033[36m📊 Executable size: ${FILE_SIZE_MB} MB\033[0m"
        
        FILE_COUNT=$(find "$OUTPUT_PATH" -type f | wc -l | tr -d ' ')
        echo -e "\033[36m📦 Total files: $FILE_COUNT\033[0m"
    else
        echo -e "\033[33m⚠️  Warning: Expected executable not found at $EXE_PATH\033[0m"
    fi
    
    echo ""
    echo -e "\033[33m📋 Prerequisites for target Windows machines:\033[0m"
    echo "   • .NET 8 Runtime (Desktop)"
    echo "   • Windows 10/11 or Windows Server 2019+"
    echo ""
    echo -e "\033[33m🔗 Download .NET 8 Runtime:\033[0m"
    echo -e "\033[34m   https://dotnet.microsoft.com/download/dotnet/8.0\033[0m"
    echo ""
    echo -e "\033[33m💡 To run the application on Windows:\033[0m"
    echo "   1. Ensure .NET 8 Runtime (Desktop) is installed on Windows"
    echo "   2. Copy the entire '$OUTPUT_PATH' folder to Windows machine"
    echo "   3. Run 'teams-phonemanager.exe'"
    echo ""
    echo -e "\033[33m💡 Cross-compilation notes:\033[0m"
    echo "   • This build was created on a non-Windows system"
    echo "   • The executable should work on Windows with .NET 8 Runtime installed"
    echo "   • Test the executable on a Windows machine before distribution"
    
else
    EXIT_CODE=$?
    echo -e "\033[31m❌ Publish failed with exit code: $EXIT_CODE\033[0m"
    exit $EXIT_CODE
fi

