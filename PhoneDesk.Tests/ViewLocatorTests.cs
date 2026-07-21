using System;
using System.Linq;

namespace PhoneDesk.Tests
{
    /// <summary>
    /// Guards the VM-first <c>ViewLocator</c> that replaced the per-View service locator:
    /// every navigable page ViewModel must resolve to a real View via the naming convention
    /// (…ViewModels.XxxViewModel → …Views.XxxView), or navigation would render "View not found".
    /// </summary>
    public class ViewLocatorTests
    {
        [Fact]
        public void Every_page_view_model_resolves_to_a_view_via_the_convention()
        {
            var assembly = typeof(PhoneDesk.ViewModels.MainWindowViewModel).Assembly;

            var pageViewModels = assembly.GetTypes()
                .Where(t => t.Namespace == "PhoneDesk.ViewModels"
                            && t.Name.EndsWith("ViewModel", StringComparison.Ordinal)
                            && t.Name != "ViewModelBase"
                            && t.Name != "MainWindowViewModel" // the shell window, not a navigated page
                            && !t.IsAbstract)
                .ToArray();

            Assert.NotEmpty(pageViewModels);

            foreach (var vm in pageViewModels)
            {
                // Mirror ViewLocator.Build's convention exactly.
                var viewName = vm.FullName!
                    .Replace("ViewModels", "Views", StringComparison.Ordinal)
                    .Replace("ViewModel", "View", StringComparison.Ordinal);

                var viewType = assembly.GetType(viewName);
                Assert.True(
                    viewType is not null,
                    $"ViewLocator convention: no View '{viewName}' found for ViewModel '{vm.FullName}'.");
            }
        }
    }
}
