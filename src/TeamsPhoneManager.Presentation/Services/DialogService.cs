using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    public class DialogService : IDialogService
    {
        private static readonly FontFamily MonospaceFont = new("Cascadia Code, Consolas, Courier New, monospace");

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

            System.Diagnostics.Debug.WriteLine($"DialogService: Cannot show confirmation dialog - window not available. Title: {title}");
            return false;
        }

        public async Task<bool> ShowScriptPreviewAsync(string title, string script)
        {
            var window = GetMainWindow();
            if (window == null) return false;

            var panel = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Review the PowerShell script that will be executed:", TextWrapping = TextWrapping.Wrap },
                    CreateScriptViewer(script, maxHeight: 400, minHeight: 200)
                }
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = panel,
                PrimaryButtonText = "Execute",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await dialog.ShowAsync(window);
            return result == ContentDialogResult.Primary;
        }

        public async Task<bool> ShowConfirmationWithPreviewAsync(string title, string message, string script)
        {
            var window = GetMainWindow();
            if (window == null) return false;

            var panel = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.OrangeRed,
                        FontWeight = FontWeight.SemiBold
                    },
                    new TextBlock { Text = "Script to be executed:", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) },
                    CreateScriptViewer(script, maxHeight: 300, minHeight: 150)
                }
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = panel,
                PrimaryButtonText = "Confirm & Execute",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await dialog.ShowAsync(window);
            return result == ContentDialogResult.Primary;
        }

        private static ScrollViewer CreateScriptViewer(string script, double maxHeight, double minHeight)
        {
            var textBox = new TextBox
            {
                Text = script,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                FontFamily = MonospaceFont,
                FontSize = 12,
                MaxHeight = maxHeight,
                MinHeight = minHeight,
                Watermark = "PowerShell Script"
            };

            return new ScrollViewer
            {
                Content = textBox,
                MaxHeight = maxHeight,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
            };
        }
    }
}
