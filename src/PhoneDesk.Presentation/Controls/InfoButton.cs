using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using FluentIcons.Avalonia;
using FluentIcons.Common;

namespace PhoneDesk.Controls;

/// <summary>Optional help available on hover, keyboard focus, or click; never hides required actions.</summary>
public class InfoButton : Button
{
    public static readonly StyledProperty<string?> HelpTitleProperty =
        AvaloniaProperty.Register<InfoButton, string?>(nameof(HelpTitle));
    public static readonly StyledProperty<string?> HelpTextProperty =
        AvaloniaProperty.Register<InfoButton, string?>(nameof(HelpText));

    public string? HelpTitle { get => GetValue(HelpTitleProperty); set => SetValue(HelpTitleProperty, value); }
    public string? HelpText { get => GetValue(HelpTextProperty); set => SetValue(HelpTextProperty, value); }

    protected override Type StyleKeyOverride => typeof(Button);

    public InfoButton()
    {
        Width = 28;
        Height = 28;
        Padding = new Thickness(4);
        Background = Brushes.Transparent;
        BorderThickness = new Thickness(0);
        VerticalAlignment = VerticalAlignment.Center;
        Content = new SymbolIcon { Symbol = Symbol.Info, FontSize = 17 };
        Bind(AutomationProperties.NameProperty, new Binding(nameof(HelpTitle)) { Source = this });
        Bind(AutomationProperties.HelpTextProperty, new Binding(nameof(HelpText)) { Source = this });
        ToolTip.SetTip(this, CreateHelpContent());
        ToolTip.SetShowDelay(this, 0);
        Flyout = new Flyout { Content = CreateHelpContent(), Placement = PlacementMode.BottomEdgeAlignedLeft };
        Click += (_, _) => ToolTip.SetIsOpen(this, false);
        GotFocus += (_, e) =>
        {
            if (e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional)
                ToolTip.SetIsOpen(this, true);
        };
        LostFocus += (_, _) => ToolTip.SetIsOpen(this, false);
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && (ToolTip.GetIsOpen(this) || Flyout?.IsOpen == true))
            {
                ToolTip.SetIsOpen(this, false);
                Flyout?.Hide();
                e.Handled = true;
            }
        };
    }

    private StackPanel CreateHelpContent()
    {
        var title = new TextBlock { FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap };
        title.Bind(TextBlock.TextProperty, new Binding(nameof(HelpTitle)) { Source = this });
        var text = new TextBlock { TextWrapping = TextWrapping.Wrap };
        text.Bind(TextBlock.TextProperty, new Binding(nameof(HelpText)) { Source = this });
        return new StackPanel { MaxWidth = 380, Spacing = 6, Children = { title, text } };
    }
}
