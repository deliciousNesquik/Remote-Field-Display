using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using RFD.Interfaces;
using RFD.UserControls;

namespace RFD.Services;

public class WindowService : IWindowService
{
    public void OpenWindow<T>(T content, string title, int width = 500, int height = 400) where T : Control
    {
        
        var window = new TemplateWindow()
        {
            Title = title,
            Content = content,
            Width = width,
            Height = height
        };
        window.Show();
    }
}