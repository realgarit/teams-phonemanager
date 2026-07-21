using Avalonia.Controls;
using PhoneDesk.ViewModels;

namespace PhoneDesk.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            // Trigger the initial async load on first display. When the session cache already holds a
            // topology the command returns instantly (issue #64: navigation back is instant); otherwise
            // it performs the first read-only query with progress + cancellation.
            Loaded += (_, _) =>
            {
                if (DataContext is DashboardViewModel vm && vm.LoadCommand.CanExecute(null))
                {
                    vm.LoadCommand.Execute(null);
                }
            };
        }
    }
}
