# Teams Phone Manager — Copilot Instructions

## Architecture (Clean Architecture)

4 layers under `src/`:
- `TeamsPhoneManager.Domain` — framework-free (rules, value objects, constants, holiday computus, `LogLevel`, `IPhoneManagerVariables`/`IHolidayEntry`/`IDaySchedule` contracts)
- `TeamsPhoneManager.Application` — ports (interfaces) + use cases (`ValidationService`)
- `TeamsPhoneManager.Infrastructure` — PowerShell/MSAL adapters (only layer with `System.Management.Automation` and `Microsoft.Identity.Client`)
- `TeamsPhoneManager.Presentation` — Avalonia/MVVM UI (depends only on Domain + Application)
- `teams-phonemanager` (repo root exe) — composition root (DI, app.manifest, version, Modules content)

**Dependency Rule**: enforced by `DependencyRuleTests` — forbidden inward deps fail the build.
**ObservableObject models** = Presentation concern. **VM-first ViewLocator** (no service locator).

## Frozen / Do Not Touch

### Graph/PowerShell script builders & auth
The `ScriptBuilders` in `src/TeamsPhoneManager.Infrastructure/ScriptBuilders/` and the auth flow (`MsalGraphAuthenticationService`, `PowerShellContextService`) are **off-limits** — do NOT change behavior or emitted text without asking. Structural changes (relocating, interfaces) OK if output stays byte-identical.

### macOS signing cert
The self-signed cert "Teams Phone Manager Self-Signed" (stable designated requirement `identifier "ch.realgar.teams-phonemanager" and certificate leaf = H"965282c9..."`) keeps MSAL keychain items / TCC grants across upgrades. **NEVER regenerate** — p12 at `~/.teams-phonemanager-signing/signing.p12`, also in repo secrets.

## UI / Icons

At 16px, dense FluentIcons glyphs (e.g. `PeopleTeam`, 3 figures) look thicker than sparse outlines. **Prefer simple outline glyphs** (1-2 subjects) for nav/buttons. Filled icons are OK on tinted badge circles (`.icon-badge` style), empty states, brand marks — solid glyphs can't have stroke-weight mismatches.

**Brand**: T-Pad monogram (dial-pad dots, "T" at full white, side dots 38% opacity, diagonal gradient **#7B80EE → #45478F** + top sheen). Canonical source `assets/icon.svg`; all derived icons regenerate via `assets/generate-icons.py`; `BrandGradientBrush` in App.axaml.

README screenshots at `docs/screenshots/` (2560×1496 = 1280×720 @2x + synthetic macOS title bar) regenerate via `ScreenshotGenerator.cs` (xunit `[Fact]`, only runs with `GENERATE_SCREENSHOTS=1`, uses Avalonia.Headless + real Skia, stubs services, validates frames before overwriting).

## Release Workflow

- **Release-please-action**: Must use **v5** (SHA: `45996ed1f6d02564a971a2fa1b5860e934307cf7`), NOT v4. v4 is deprecated (Node 20) and causes issues.
- **Config file**: `release-please-config.json`
- **Manifest file**: `.release-please-manifest.json`
- **Workflow file**: `.github/workflows/build.yml`
- **Version source**: `teams-phonemanager.csproj` `<Version>` — also in `app.manifest` and `ConstantsService.cs`. Bump via `Scripts/bump-version.sh minor|patch|major`.

### Phantom v4.0.0 PR — root fix & prevention

Release-please repeatedly created v4.0.0 PRs because old squashed commit `0406e3d` contained `BREAKING CHANGE: ViewModel constructors now require ISharedStateService and IDialogService parameters`. 

**Permanent fix**: `last-release-sha` in `release-please-config.json` anchors release-please to only scan commits after the specified SHA. The value points to the v3.21.5 release commit — any BREAKING CHANGE footers before it are ignored.

**Do NOT reintroduce `BREAKING CHANGE:` footers unless you actually intend a major bump.**

### macOS signing & Homebrew delivery

- Sign publish output BEFORE .app assembly (`Scripts/package-macos.sh`); bundle is never sealed (codesign rejects .NET managed-dll layout as nested code).
- Homebrew cask (`realgarit/homebrew-tap`) needs `postflight` stripping `com.apple.quarantine` — Homebrew quarantines cask downloads and Gatekeeper SIGKILLs (exit 137) quarantined ad-hoc/self-signed binaries.
- `bump-homebrew-cask` job pushes via `PAT_TOKEN`.

## CI

- Build runs on Windows, macOS (x64 + arm64), Linux
- Windows: Inno Setup installer; macOS: ad-hoc signed .app bundle; Linux: zip
- **PR checkout**: `github.event.pull_request.head.sha` (not merge ref) to avoid race where merge ref is deleted before runner picks up the job
- `dotnet test` skips screenshot gen unless `GENERATE_SCREENSHOTS=1`

## Workflow Convention (PR workflow)

Always: **branch → commit → push + open PR → CI green → merge (merge commit, not squash) → clean up ALL branches** (remote delete, local branches, stale worktrees). Always end a working session with a version bump (the bump IS the release trigger). Do NOT create tags manually — build.yml auto-creates them.