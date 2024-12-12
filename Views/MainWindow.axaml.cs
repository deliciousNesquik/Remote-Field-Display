using Avalonia.Controls;
using RFD.ViewModels;

namespace RFD.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}