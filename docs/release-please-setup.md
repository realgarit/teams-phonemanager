# Release Please Configuration

This project uses [release-please](https://github.com/googleapis/release-please) (via the
`googleapis/release-please-action` GitHub Action) to automate versioning and release
management. Every push to `main` triggers RP to evaluate conventional commit messages
since the last release and propose a release PR.

## Version pinning (fixed at v4)

RP is pinned to **v4** of `googleapis/release-please-action` via an explicit commit SHA:

```
uses: googleapis/release-please-action@8b8fd2cc23b2e18957157a9d923d75aa0c6f6ad5 # v4
```

We deliberately do **not** use a floating tag (`v4`) or a version range (`>=4`). This
prevents Dependabot or GitHub's own action resolver from silently upgrading the action to
v5, which would trigger phantom major-version bumps (e.g. 3.x → 4.0.0) because v5 uses a
different release-please CLI that re-evaluates the entire commit history.

## Preventing phantom major-version bumps

The most critical setting is **`last-release-sha`** in `.release-please-manifest.json`:

```json
{
  ".": "3.21.3",
  "last-release-sha": "dc640485e0a866596b0eb6919918a4a05140b4f6"
}
```

* `".": "3.21.3"` — the current package version (release-please format).
* `"last-release-sha"` — the commit SHA of the last released commit. RP only evaluates
  commits *after* this SHA for the next release, preventing it from re-scanning the
  entire history and proposing a breaking-change major bump every time it runs.

**Whenever you push a tagged release commit**, update `last-release-sha` to point at that
commit so RP does not re-propose the same release on the next push to `main`.

## Four-digit version fields for Windows

The .NET project uses these MSBuild properties in `teams-phonemanager.csproj`:

```xml
<Version>3.21.3</Version>            <!-- SemVer — must match RP tag exactly -->
<AssemblyVersion>3.21.3.0</AssemblyVersion>   <!-- 4-part for Windows compatibility -->
<FileVersion>3.21.3.0</FileVersion>           <!-- 4-part for Windows compatibility -->
```

- **`<Version>`** is 3-part SemVer and must match the release-please tag (e.g. `v3.21.3`).
- **`<AssemblyVersion>` / `<FileVersion>`** use 4-part versions (e.g. `3.21.3.0`) because
  Windows requires four digits. The CI validation in `build.yml` accepts either `X.Y.Z` or
  `X.Y.Z.0` for these properties (see the `validate-release` job).

## Key files

| File | Purpose |
|------|---------|
| `.github/workflows/build.yml` | CI/CD pipeline: RP, validation, build, publish, Homebrew |
| `release-please-config.json` | RP bootstrap SHA, changelog sections, release-type |
| `.release-please-manifest.json` | Current version + `last-release-sha` |
| `version.txt` | Human-readable version (must match RP version) |
| `teams-phonemanager.csproj` | Project `<Version>`, `<AssemblyVersion>`, `<FileVersion>` |

## What happens on a push to `main`

1. **`release-please` job**: RP evaluates commits since `last-release-sha`, creates or
   updates a release PR (branch: `release-please--branches--main`).
2. **PR validation**: The `pr-validate` job builds + tests the PR across all runtimes
   (Windows, macOS Intel, macOS ARM, Linux).
3. **When the release PR is merged**: RP creates a Git tag and a draft GitHub release,
   then the full `build` → `upload-release-assets` → `bump-homebrew-cask` pipeline runs.

## Troubleshooting

### RP keeps creating duplicate or phantom release PRs
- Verify `.release-please-manifest.json` has `"last-release-sha"` pointing to the
  **exact commit** of the most recent release tag.
- Verify the action is pinned to a **v4 commit SHA**, not a floating `v4` tag.
- Delete stale RP branches (`release-please--branches--main-*`) and re-run.

### Validate-release fails with "Version is not a stable semantic version"
- Ensure the release tag matches `vX.Y.Z` format (no pre-release suffixes, no fourth
  digit).
- Check that `<Version>` in `teams-phonemanager.csproj` matches the tag exactly (3 parts).

### Windows builds fail on version mismatch
- `<AssemblyVersion>` and `<FileVersion>` must be 4-part (e.g. `3.21.3.0`). Windows
  does not accept 3-part assembly versions.