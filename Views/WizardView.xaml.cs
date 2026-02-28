using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class WizardView : UserControl
    {
        public WizardView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<WizardViewModel>();
        }
    }
}
