using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    /// <summary>
    /// Avalonia implementation of the dialog service.
    /// This class contains UI framework dependencies and should be registered in DI.
    /// </summary>
    public class DialogService : IDialogService
    {
        private Window? GetMainWindow()
        {
            return Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var window = GetMainWindow();
            if (window != null)
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                };
                await dialog.ShowAsync(window);
            }
            else
            {
                // Log when window is not available
                System.Diagnostics.Debug.WriteLine($"DialogService: Cannot show message dialog - window not available. Title: {title}");
            }
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var window = GetMainWindow();
            if (window != null)
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary
                };
                var result = await dialog.ShowAsync(window);
                return result == ContentDialogResult.Primary;
            }
            
            // Log warning when window is not available and return false
            System.Diagnostics.Debug.WriteLine($"DialogService: Cannot show confirmation dialog - window not available. Title: {title}");
            return false;
        }
    }
}
