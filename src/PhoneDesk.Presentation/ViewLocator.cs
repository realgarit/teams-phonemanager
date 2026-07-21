using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PhoneDesk.ViewModels;

namespace PhoneDesk
{
    /// <summary>
    /// VM-first view resolution: maps a ViewModel instance to its View by naming convention
    /// (…ViewModels.XxxViewModel → …Views.XxxView). Registered in App.axaml's DataTemplates so a
    /// ContentControl bound to a ViewModel renders the matching View, with the ViewModel as its
    /// DataContext. Replaces the previous per-View `Program.Services.GetService<…>()` service-locator.
    /// </summary>
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object? data)
        {
            if (data is null)
                return new TextBlock { Text = "No view-model" };

            var name = data.GetType().FullName!
                .Replace("ViewModels", "Views", StringComparison.Ordinal)
                .Replace("ViewModel", "View", StringComparison.Ordinal);

            var type = Type.GetType(name);
            if (type is not null)
                return (Control)Activator.CreateInstance(type)!;

            return new TextBlock { Text = "View not found: " + name };
        }

        public bool Match(object? data) => data is ViewModelBase;
    }
}
