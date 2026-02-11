# Teams Phone Manager

An Avalonia-based Microsoft Teams Phone Manager application built with .NET 10 to simplify Microsoft Teams Phone System administration tasks. Cross-platform support for Windows and macOS.

## Features

- Auto Attendants Management
- Call Queues Management
- Holiday Management
- 365 Groups Integration
- Resource Accounts
- Modern UI

## Prerequisites

- Windows 10/11, Windows Server 2019+, or macOS 10.15+
- .NET 10 Runtime (Desktop) - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- Microsoft Teams & Graph PowerShell Module (included with the app)
- PowerShell 7.4+ (included with the app)

## Development Setup

1. Install .NET 10 SDK:
   ```bash
   # Download from: https://dotnet.microsoft.com/download/dotnet/10.0
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the application:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

## Usage

1. Download from the latest release
2. Run the application:
   - Windows: `teams-phonemanager.exe`
   - macOS: `teams-phonemanager` (or the .app bundle)

## Architecture

- .NET 10 Avalonia (cross-platform UI framework)
- Material Design for Avalonia
- MVVM Pattern
- Microsoft.PowerShell.SDK
- Custom logging service for debugging and monitoring

## Project Structure

```
teams-phonemanager/
├── Models/              # Data models
├── ViewModels/          # MVVM ViewModels
├── Views/               # Avalonia Views (AXAML)
├── Services/            # Business logic services
├── Helpers/             # Utility classes and converters
├── Resources/           # Application resources
├── Scripts/             # Publish & Download pwsh Modules
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Screenshots
<img width="1792" height="1064" alt="SCR-20251122-nefg" src="https://github.com/user-attachments/assets/38fac33a-56a1-4380-9fb3-f47e4902c40c" />
<img width="1792" height="1064" alt="SCR-20251122-nelf" src="https://github.com/user-attachments/assets/7de0b4e6-d7d5-4849-818c-d41aaeaa5d5f" />
<img width="1792" height="1064" alt="SCR-20251122-nenb" src="https://github.com/user-attachments/assets/5ec138f7-3384-4bb5-b6aa-4085e05676fd" />
<img width="1792" height="1064" alt="SCR-20251122-neqo" src="https://github.com/user-attachments/assets/28d691df-3cf1-4983-84ed-00994a47405d" />
<img width="1792" height="1064" alt="SCR-20251122-nest" src="https://github.com/user-attachments/assets/0161502d-034b-4ae2-ab0a-96c456a77329" />
<img width="1792" height="1064" alt="SCR-20251122-nexk" src="https://github.com/user-attachments/assets/aad1408e-60eb-42d9-b55b-8e3beaebf4cf" />
<img width="1792" height="1064" alt="SCR-20251122-neyt" src="https://github.com/user-attachments/assets/f2ab416d-bc7a-4267-8394-77667d725b17" />
<img width="1792" height="1064" alt="SCR-20251122-nfad" src="https://github.com/user-attachments/assets/1b8d014d-6b09-4996-a148-fce96663ec62" />
<img width="1792" height="1064" alt="SCR-20251122-nfbp" src="https://github.com/user-attachments/assets/b4ac4949-8104-4ef7-bece-43955ca26550" />
<img width="1792" height="1064" alt="SCR-20251122-nfcz" src="https://github.com/user-attachments/assets/e7b8d45f-f0b8-433d-a729-fe2718d4ee6a" />
