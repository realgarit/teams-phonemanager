using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class WelcomeView : UserControl
    {
        public WelcomeView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<WelcomeViewModel>();
        }
    }
} 
