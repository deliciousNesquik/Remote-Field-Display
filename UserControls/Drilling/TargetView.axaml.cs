using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace RFD.UserControls;

public partial class TargetSection : UserControl
{
    public TargetSection()
    {
        InitializeComponent();
    }

    public void Wrap()
    { 
        FrameAndNameStackPanel.IsVisible = false;
    }

    public void Unwrap()
    {
        FrameAndNameStackPanel.IsVisible = true;
    }
}