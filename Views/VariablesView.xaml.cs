using System.Windows.Controls;

namespace teams_phonemanager.Views
{
    public partial class VariablesView : UserControl
    {
        public VariablesView()
        {
            InitializeComponent();
            DataContext = new ViewModels.VariablesViewModel();
        }
    }
} 
