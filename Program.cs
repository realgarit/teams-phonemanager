using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using System;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager;

class Program
{
    public static IServiceProvider? Services { get; private set; }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core services (singletons for app lifetime)
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPowerShellContextService, PowerShellContextService>();

        // Transient services (new instance per request)
        services.AddTransient<IPowerShellCommandService, PowerShellCommandService>();
        services.AddTransient<IValidationService, ValidationService>();
        services.AddTransient<IErrorHandlingService, ErrorHandlingService>();
        services.AddTransient<IPowerShellSanitizationService, PowerShellSanitizationService>();

        // ViewModels (transient - new instance per navigation)
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<WelcomeViewModel>();
        services.AddTransient<GetStartedViewModel>();
        services.AddTransient<VariablesViewModel>();
        services.AddTransient<M365GroupsViewModel>();
        services.AddTransient<CallQueuesViewModel>();
        services.AddTransient<AutoAttendantsViewModel>();
        services.AddTransient<HolidaysViewModel>();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}

