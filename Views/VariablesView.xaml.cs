using System.Windows.Controls;
using teams_phonemanager.ViewModels;
using teams_phonemanager.Services;

namespace teams_phonemanager.Views
{
    public partial class VariablesView : UserControl
    {
        public VariablesView()
        {
            InitializeComponent();
            DataContext = new VariablesViewModel(
                PowerShellService.Instance,
                LoggingService.Instance,
                SessionManager.Instance);
        }
    }
} 