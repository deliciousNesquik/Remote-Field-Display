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
    }
    public ManualConnectionDialog(ManualConnectionDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}