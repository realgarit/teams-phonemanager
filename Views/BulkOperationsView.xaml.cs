using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class BulkOperationsView : UserControl
    {
        public BulkOperationsView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<BulkOperationsViewModel>();
        }
    }
}
