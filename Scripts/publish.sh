#!/bin/bash
# Teams Phone Manager - Publish Script for macOS/Linux
# Builds and publishes the application for the current platform

set -e

# Default values
CONFIGURATION="Release"
RUNTIME=""
OUTPUT_PATH="./publish/framework-dependent"
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
    echo "Teams Phone Manager - Publish Script" | grep -q . && echo -e "\033[32mTeams Phone Manager - Publish Script\033[0m"
    echo ""
    echo -e "\033[33mUsage: ./Scripts/publish.sh [options]\033[0m"
    echo ""
    echo -e "\033[33mOptions:\033[0m"
    echo "  -Configuration <config>  Build configuration (Debug|Release). Default: Release"
    echo "  -Runtime <runtime>       Target runtime (osx-x64|osx-arm64|linux-x64). Default: auto-detect"
    echo "  -OutputPath <path>       Output directory. Default: ./publish/framework-dependent (repo root)"
    echo "  -Clean                   Clean output directory before publishing"
    echo "  -Help                    Show this help message"
    echo ""
    echo -e "\033[33mExamples:\033[0m"
    echo "  ./Scripts/publish.sh                                    # Default publish"
    echo "  ./Scripts/publish.sh -Configuration Debug              # Debug build"
    echo "  ./Scripts/publish.sh -Runtime osx-arm64               # Apple Silicon build"
    echo "  ./Scripts/publish.sh -Clean                            # Clean publish"
    exit 0
fi

# Resolve repository root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo -e "\033[32mTeams Phone Manager - Publishing Framework-Dependent Deployment\033[0m"
echo "================================================================"
echo ""

# Check for .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo -e "\033[31mError: .NET SDK not found. Please install .NET 8 SDK.\033[0m"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo -e "\033[36mUsing .NET SDK version: $DOTNET_VERSION\033[0m"

# Auto-detect runtime if not specified
if [ -z "$RUNTIME" ]; then
    ARCH=$(uname -m)
    OS=$(uname -s)
    
    if [ "$OS" = "Darwin" ]; then
        if [ "$ARCH" = "arm64" ]; then
            RUNTIME="osx-arm64"
        else
            RUNTIME="osx-x64"
        fi
    elif [ "$OS" = "Linux" ]; then
        RUNTIME="linux-x64"
    else
        echo -e "\033[31mError: Unsupported operating system: $OS\033[0m"
        exit 1
    fi
    echo -e "\033[36mAuto-detected runtime: $RUNTIME\033[0m"
fi

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
echo ""

echo -e "\033[33mStarting publish process...\033[0m"

# Change to repo root for dotnet publish
cd "$REPO_ROOT"

# Build publish command
# Override OutputType for macOS/Linux (WinExe is Windows-specific)
PUBLISH_CMD=(
    "publish"
    "--configuration" "$CONFIGURATION"
    "--runtime" "$RUNTIME"
    "--output" "$OUTPUT_PATH"
    "--self-contained" "false"
    "/p:OutputType=Exe"
    "--verbosity" "minimal"
)

echo -e "\033[90mExecuting: dotnet ${PUBLISH_CMD[*]}\033[0m"

# Run dotnet publish
if dotnet "${PUBLISH_CMD[@]}"; then
    echo ""
    echo -e "\033[32m✅ Publish completed successfully!\033[0m"
    echo ""
    
    # Find the executable (name depends on project name)
    EXE_NAME="teams-phonemanager"
    EXE_PATH="$OUTPUT_PATH/$EXE_NAME"
    
    if [ -f "$EXE_PATH" ]; then
        FILE_SIZE=$(stat -f%z "$EXE_PATH" 2>/dev/null || stat -c%s "$EXE_PATH" 2>/dev/null)
        FILE_SIZE_MB=$(echo "scale=2; $FILE_SIZE / 1048576" | bc)
        echo -e "\033[36m📁 Output directory: $OUTPUT_PATH\033[0m"
        echo -e "\033[36m📄 Executable: $EXE_NAME\033[0m"
        echo -e "\033[36m📊 Executable size: ${FILE_SIZE_MB} MB\033[0m"
        
        FILE_COUNT=$(find "$OUTPUT_PATH" -type f | wc -l | tr -d ' ')
        echo -e "\033[36m📦 Total files: $FILE_COUNT\033[0m"
    fi
    
    echo ""
    echo -e "\033[33m📋 Prerequisites for target machines:\033[0m"
    echo "   • .NET 8 Runtime"
    if [ "$RUNTIME" = "osx-arm64" ] || [ "$RUNTIME" = "osx-x64" ]; then
        echo "   • macOS 10.15+ or macOS 11+"
    elif [ "$RUNTIME" = "linux-x64" ]; then
        echo "   • Linux with glibc 2.31+"
    fi
    echo ""
    echo -e "\033[33m🔗 Download .NET 8 Runtime:\033[0m"
    echo -e "\033[34m   https://dotnet.microsoft.com/download/dotnet/8.0\033[0m"
    echo ""
    echo -e "\033[33m💡 To run the application:\033[0m"
    echo "   1. Ensure .NET 8 Runtime is installed"
    echo "   2. Copy the entire '$OUTPUT_PATH' folder to target machine"
    echo "   3. Run './$EXE_NAME'"
    
else
    EXIT_CODE=$?
    echo -e "\033[31m❌ Publish failed with exit code: $EXIT_CODE\033[0m"
    exit $EXIT_CODE
fi

