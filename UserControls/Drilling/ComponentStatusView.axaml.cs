using Avalonia.Controls;

namespace RFD.UserControls;

public partial class StatusSection : UserControl
{
    public StatusSection()
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