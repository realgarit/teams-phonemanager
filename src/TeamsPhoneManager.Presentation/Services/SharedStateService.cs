using CommunityToolkit.Mvvm.ComponentModel;
using teams_phonemanager.Models;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services;

/// <summary>
/// Shared state service that holds application-wide state.
/// Registered as a singleton so all ViewModels share the same instance.
/// </summary>
public partial class SharedStateService : ObservableObject, ISharedStateService
{
    [ObservableProperty]
    private PhoneManagerVariables _variables = new();

    [ObservableProperty]
    private bool _skipScriptPreview;

    [ObservableProperty]
    private bool _skipDeleteConfirmation;

    [ObservableProperty]
    private bool _autoRefreshAfterOperations = true;

    [ObservableProperty]
    private LogLevel _minimumLogLevel = LogLevel.Info;
}
