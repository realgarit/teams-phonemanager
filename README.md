# Teams Phone Manager

A WPF-based Microsoft Teams Phone Manager application built with .NET 8 to simplify Microsoft Teams Phone System administration tasks.

## Features

- Auto Attendants Management
- Call Queues Management
- Holiday Management
- 365 Groups Integration
- Resource Accounts
- Modern UI

## Prerequisites

- Windows 10/11 or Windows Server 2019+
- .NET 8 Runtime (Desktop) - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft Teams & Graph PowerShell Module (installed automatically)
- PowerShell 7.4+ (included with the application)

## Installation & Deployment

#### Publishing the Application

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd teams-phonemanager
   ```

2. Publish using the provided script:
   ```powershell
   .\publish.ps1
   ```

   Or with custom options:
   ```powershell
   .\publish.ps1 -Help
   ```

3. Deploy to target machines:
   - Copy the entire `publish\framework-dependent` folder to the target machine
   - Ensure .NET 8 Runtime is installed
   - Run `teams-phonemanager.exe`

### Development Setup

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
2. Run `teams-phonemanager.exe`

## Architecture

- .NET 8 WPF
- Material Design for WPF
- MVVM Pattern
- Microsoft.PowerShell.SDK
- Custom logging service for debugging and monitoring

## Project Structure

```
teams-phonemanager/
├── Models/                 # Data models
├── ViewModels/            # MVVM ViewModels
├── Views/                 # WPF Views (XAML)
├── Services/              # Business logic services
├── Helpers/               # Utility classes and converters
├── Resources/             # Application resources
└── publish.ps1           # Publishing script
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

![built-a-wpf-based-teams-phone-manager-to-simplify-admin-v0-ry2k65wn95ve1](https://github.com/user-attachments/assets/808f7d49-1915-4f7f-aa26-5ace45e5fe9e)
![built-a-wpf-based-teams-phone-manager-to-simplify-admin-v0-k8os74wn95ve1](https://github.com/user-attachments/assets/972751a6-17f0-4528-9d66-8b95abbe85ec)
![built-a-wpf-based-teams-phone-manager-to-simplify-admin-v0-ihvem4wn95ve1](https://github.com/user-attachments/assets/339afd0c-e8fc-4a4e-9ca8-ce7dd24e8169)
![built-a-wpf-based-teams-phone-manager-to-simplify-admin-v0-xjnri4wn95ve1](https://github.com/user-attachments/assets/840bfd43-84f1-4ac1-a18b-730224b184e1)
<img width="960" height="516" alt="2025-10-08 23_01_53-Teams Phone Manager" src="https://github.com/user-attachments/assets/7acf025a-87f9-4726-892c-1e9a745fa7e5" />
<img width="960" height="516" alt="2025-10-08 23_00_34-Teams Phone Manager" src="https://github.com/user-attachments/assets/f40a3c07-0098-454d-933e-441a31478c87" />
<img width="960" height="516" alt="2025-10-08 23_02_01-Teams Phone Manager" src="https://github.com/user-attachments/assets/dcb41fd7-5a57-414a-9d0b-8b8622027578" />
<img width="960" height="516" alt="2025-10-08 23_02_17-Teams Phone Manager" src="https://github.com/user-attachments/assets/2c0fa87f-ccbf-407a-b2a4-cd2298947528" />
<img width="960" height="516" alt="2025-10-08 23_02_23-Teams Phone Manager" src="https://github.com/user-attachments/assets/4a8732df-3a3f-4527-86bc-0170a47094dc" />
![built-a-wpf-based-teams-phone-manager-to-simplify-admin-v0-5pqqf4wn95ve1](https://github.com/user-attachments/assets/c4785e70-39a2-4db5-96f9-e8e1fdd384b0)