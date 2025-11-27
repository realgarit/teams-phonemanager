#!/bin/bash
# Version bump script for semantic versioning
# Usage: ./Scripts/bump-version.sh [major|minor|patch] [commit-message] [--push] [--branch=dev]

set -e

# Default values
BUMP_TYPE=""
COMMIT_MESSAGE=""
PUSH=false
BRANCH="dev"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        major|minor|patch)
            BUMP_TYPE="$1"
            shift
            ;;
        --push)
            PUSH=true
            shift
            ;;
        --branch=*)
            BRANCH="${1#*=}"
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [major|minor|patch] [commit-message] [--push] [--branch=dev]"
            echo ""
            echo "Examples:"
            echo "  $0 patch 'Fix bug in validation'"
            echo "  $0 minor --push"
            echo "  $0 major --push --branch=main"
            exit 0
            ;;
        *)
            if [ -z "$COMMIT_MESSAGE" ]; then
                COMMIT_MESSAGE="$1"
            else
                echo "Unknown option: $1"
                exit 1
            fi
            shift
            ;;
    esac
done

# Validate bump type
if [ -z "$BUMP_TYPE" ]; then
    echo "Error: Bump type (major|minor|patch) is required"
    echo "Usage: $0 [major|minor|patch] [commit-message] [--push]"
    exit 1
fi

# Get repo root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT"

# Read current version from csproj
CSPROJ_PATH="$REPO_ROOT/teams-phonemanager.csproj"
if [ ! -f "$CSPROJ_PATH" ]; then
    echo "Error: teams-phonemanager.csproj not found"
    exit 1
fi

CURRENT_VERSION=$(grep '<Version>' "$CSPROJ_PATH" | sed -n 's/.*<Version>\([0-9.]*\)<\/Version>.*/\1/p' | head -1)

if [ -z "$CURRENT_VERSION" ]; then
    echo "Error: Could not find version in csproj file"
    exit 1
fi

echo -e "\033[36mCurrent version: $CURRENT_VERSION\033[0m"

# Parse version
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION"
MAJOR=${VERSION_PARTS[0]}
MINOR=${VERSION_PARTS[1]}
PATCH=${VERSION_PARTS[2]}

# Bump version
case $BUMP_TYPE in
    major)
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        ;;
    minor)
        MINOR=$((MINOR + 1))
        PATCH=0
        ;;
    patch)
        PATCH=$((PATCH + 1))
        ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"
echo -e "\033[32mNew version: $NEW_VERSION\033[0m"

# Update csproj file
sed -i.bak "s/<Version>$CURRENT_VERSION<\/Version>/<Version>$NEW_VERSION<\/Version>/g" "$CSPROJ_PATH"
sed -i.bak "s/<AssemblyVersion>$CURRENT_VERSION<\/AssemblyVersion>/<AssemblyVersion>$NEW_VERSION<\/AssemblyVersion>/g" "$CSPROJ_PATH"
sed -i.bak "s/<FileVersion>$CURRENT_VERSION<\/FileVersion>/<FileVersion>$NEW_VERSION<\/FileVersion>/g" "$CSPROJ_PATH"
rm -f "$CSPROJ_PATH.bak"
echo -e "\033[32m✓ Updated teams-phonemanager.csproj\033[0m"

# Update app.manifest
MANIFEST_PATH="$REPO_ROOT/app.manifest"
sed -i.bak "s/version=\"$CURRENT_VERSION\"/version=\"$NEW_VERSION\"/g" "$MANIFEST_PATH"
rm -f "$MANIFEST_PATH.bak"
echo -e "\033[32m✓ Updated app.manifest\033[0m"

# Update ConstantsService.cs
CONSTANTS_PATH="$REPO_ROOT/Services/ConstantsService.cs"
sed -i.bak "s/public const string Version = \"Version $CURRENT_VERSION\";/public const string Version = \"Version $NEW_VERSION\";/g" "$CONSTANTS_PATH"
rm -f "$CONSTANTS_PATH.bak"
echo -e "\033[32m✓ Updated ConstantsService.cs\033[0m"

# Stage changes
git add teams-phonemanager.csproj app.manifest Services/ConstantsService.cs

# Create commit message if not provided
if [ -z "$COMMIT_MESSAGE" ]; then
    COMMIT_MESSAGE="Bump version to $NEW_VERSION ($BUMP_TYPE)"
fi

# Commit
git commit -m "$COMMIT_MESSAGE"
echo -e "\033[32m✓ Committed version bump\033[0m"

# Push if requested
if [ "$PUSH" = true ]; then
    git push origin "$BRANCH"
    echo -e "\033[32m✓ Pushed to origin/$BRANCH\033[0m"
fi

echo ""
echo -e "\033[32mVersion bumped from $CURRENT_VERSION to $NEW_VERSION\033[0m"
echo -e "\033[33mNext steps:\033[0m"
echo -e "\033[37m  1. Review your changes\033[0m"
if [ "$PUSH" = false ]; then
    echo -e "\033[37m  2. Push to dev: git push origin $BRANCH\033[0m"
fi
echo -e "\033[37m  3. Create PR from dev to main on GitHub\033[0m"
echo -e "\033[37m  4. After merge, release will be created automatically\033[0m"

