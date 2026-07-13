# Teams Phone Manager — Copilot Instructions

## Release Workflow

- **Release-please-action**: Must use **v5** (SHA: `45996ed1f6d02564a971a2fa1b5860e934307cf7`), NOT v4. v4 is deprecated (Node 20) and causes issues.
- **Config file**: `release-please-config.json`
- **Manifest file**: `.release-please-manifest.json`
- **Workflow file**: `.github/workflows/build.yml`

### Phantom v4.0.0 PR — root fix

Release-please was repeatedly creating v4.0.0 release PRs because an old squashed commit (`0406e3d`, now rewritten as `8159da9`) contained `BREAKING CHANGE: ViewModel constructors now require ISharedStateService and IDialogService parameters` in its commit body. Release-please scanned the full history and detected this as an un-released breaking change.

**Fix applied**: The `BREAKING CHANGE` line was surgically removed from that commit's body via `git filter-branch` and force-pushed. The commit `0406e3d` was rewritten to `8159da9`. All downstream commits were also rewritten. No `last-release-sha` config hack is needed.

**Do NOT reintroduce `BREAKING CHANGE:` footers in commit messages unless you actually intend a major version bump.**

## CI

- Build runs on Windows, macOS (x64 + arm64), Linux
- Windows uses Inno Setup for installer packaging
- macOS uses ad-hoc signing + .app bundle
- **PR checkout**: Uses `github.event.pull_request.head.sha` instead of merge ref to avoid race condition (merge refs are deleted when PR merges, causing Windows runners to fail with "couldn't find remote ref")