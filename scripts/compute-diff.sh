#!/usr/bin/env bash
# scripts/compute-diff.sh
# Writes pr.diff and changed-files.txt for the PR's diff against BASE_REF.
# Requires: BASE_REF env var (the PR's base branch name, e.g. "main").
set -euo pipefail

: "${BASE_REF:?BASE_REF env var is required}"

git diff "origin/${BASE_REF}"...HEAD > pr.diff
git diff --name-only "origin/${BASE_REF}"...HEAD > changed-files.txt
echo "Diff has $(wc -l < pr.diff) lines, $(wc -l < changed-files.txt) files changed"
