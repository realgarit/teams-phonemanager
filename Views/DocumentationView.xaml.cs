using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class DocumentationView : UserControl
    {
        public DocumentationView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<DocumentationViewModel>();
        }
    }
}
