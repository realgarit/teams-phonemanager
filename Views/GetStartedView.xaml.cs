using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class GetStartedView : UserControl
    {
        public GetStartedView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<GetStartedViewModel>();
        }
    }
} 
