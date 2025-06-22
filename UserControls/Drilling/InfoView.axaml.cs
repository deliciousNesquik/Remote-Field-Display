using Avalonia.Controls;

namespace RFD.UserControls;

public partial class InformationSection : UserControl
{
    public InformationSection()
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