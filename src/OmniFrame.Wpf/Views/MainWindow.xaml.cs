using System.Windows;
using OmniFrame.Wpf.ViewModels;

namespace OmniFrame.Wpf.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
