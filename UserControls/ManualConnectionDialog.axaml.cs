using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RFD.ViewModels;

namespace RFD.UserControls;

public partial class ManualConnectionDialog : UserControl
{
    public ManualConnectionDialog()
    {
        InitializeComponent();
        DataContext = new ManualConnectionDialogViewModel();
    }
}