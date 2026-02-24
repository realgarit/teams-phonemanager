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
            return false;
        }
    }
}
