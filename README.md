<p align="center">
  <img src="assets/banner.svg" width="800" alt="Teams Phone Manager"/>
</p>

<p align="center">
  <a href="https://github.com/realgarit/teams-phonemanager/releases/latest"><img src="https://img.shields.io/github/v/release/realgarit/teams-phonemanager?label=release&color=6A6FDD" alt="Latest release"/></a>
  <a href="https://github.com/realgarit/teams-phonemanager/actions/workflows/build.yml"><img src="https://img.shields.io/github/actions/workflow/status/realgarit/teams-phonemanager/build.yml?branch=main&label=build" alt="Build status"/></a>
  <a href="LICENSE"><img src="https://img.shields.io/github/license/realgarit/teams-phonemanager?color=blue" alt="License"/></a>
  <img src="https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-6A6FDD" alt="Platform support"/>
  <img src="https://img.shields.io/badge/.NET-10-512BD4" alt=".NET 10"/>
</p>

<p align="center">
  <b>Manage Microsoft Teams telephony — auto attendants, call queues, and phone numbers — from one desktop app.</b>
</p>

Teams Phone Manager is a self-contained .NET desktop app for administering Microsoft
Teams Phone without writing PowerShell.

## Features

- Auto attendants, call queues, holidays, resource accounts, and Microsoft 365 Groups
- Guided setup, bulk operations, and tenant documentation
- Native Windows, macOS, and Linux builds with dark and light themes

## Installation

Download the latest build from [GitHub Releases](https://github.com/realgarit/teams-phonemanager/releases/latest).
PowerShell 7 and the required Microsoft modules are included.

### Windows

Run `teams-phonemanager-win-x64-setup.exe`. It supports per-user and all-users
installation and upgrades in place. Until code signing is enabled, Windows may show a
SmartScreen warning.

Portable alternative: download `teams-phonemanager-win-x64.zip`, extract, run
`teams-phonemanager.exe`.

#### Enterprise deployment

For Intune, Configuration Manager, or an RMM:

```powershell
.\teams-phonemanager-win-x64-setup.exe /ALLUSERS /DIR="C:\Program Files\Teams Phone Manager" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
```

Use `/CURRENTUSER` for per-user deployment and `/DIR="C:\Path"` for a managed install
location. Keep the same scope and path when upgrading.

### macOS

```bash
brew tap realgarit/tap
brew trust realgarit/tap
brew install --cask teams-phonemanager
```

Update with `brew upgrade --cask teams-phonemanager`. Manual Apple Silicon and Intel
downloads are also available on the releases page.

### Linux

Download `teams-phonemanager-linux-x64.zip`, extract, then:

```bash
chmod +x teams-phonemanager
./teams-phonemanager
```

## Updates

The app checks GitHub Releases at startup and shows a non-blocking update indicator.
On Windows, updates can be downloaded, SHA-256 verified, and installed in the app.
macOS and Linux updates continue through Homebrew or GitHub Releases.

## Development

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet restore
dotnet build
dotnet test teams-phonemanager.Tests/teams-phonemanager.Tests.csproj
dotnet run
```

Built with Avalonia, CommunityToolkit.Mvvm, Microsoft.PowerShell.SDK, and MSAL.

## License

MIT © 2026 Patrik Lleshaj. See [LICENSE](LICENSE).
