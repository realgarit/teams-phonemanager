# Teams Phone Manager

An Avalonia-based Microsoft Teams Phone Manager application built with .NET 8 to simplify Microsoft Teams Phone System administration tasks. Cross-platform support for Windows and macOS.

## Features

- Auto Attendants Management
- Call Queues Management
- Holiday Management
- 365 Groups Integration
- Resource Accounts
- Modern UI

## Prerequisites

- Windows 10/11, Windows Server 2019+, or macOS 10.15+
- .NET 8 Runtime (Desktop) - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft Teams & Graph PowerShell Module (included with the app)
- PowerShell 7.4+ (included with the app)

## Development Setup

1. Install .NET 8 SDK:
   ```bash
   # Download from: https://dotnet.microsoft.com/download/dotnet/8.0
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

- .NET 8 Avalonia (cross-platform UI framework)
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
<img width="1653" height="1083" alt="image" src="https://github.com/user-attachments/assets/89f31094-5a47-4371-94b3-e8fed0b8f42c" />
<img width="1653" height="1083" alt="image" src="https://github.com/user-attachments/assets/ebf9099e-4e60-422a-b712-776be7b259cc" />
<img width="1653" height="1083" alt="image" src="https://github.com/user-attachments/assets/20dd7f36-0046-4809-a610-eaa5b411fd15" />
<img width="1653" height="1083" alt="image" src="https://github.com/user-attachments/assets/68ffc9ce-dab3-4143-903f-8f23aa9254b1" />
<img width="1653" height="1083" alt="image" src="https://github.com/user-attachments/assets/13f56487-c0b9-4212-9b04-c51d2608278a" />
<img width="1653" height="1083" alt="image" src="https://github.com/user-attachments/assets/784c42c4-7578-472b-a84a-cc3023bd1d73" />
<img width="1542" height="971" alt="image" src="https://github.com/user-attachments/assets/42099f3a-472b-40fe-bc65-7f5c99730195" />
