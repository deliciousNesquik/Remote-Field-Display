using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using RFD.Interfaces;

namespace RFD.Services;

public class WindowService : IWindowService
{
    public void OpenWindow<T>(T content, string title, IBrush background, int width = 500, int height = 400) where T : Control
    {
        var window = new Window
        {
            Title = title,
            Background = background,
            Content = content,
            Width = width,
            Height = height
        };
        window.Show();
    }
}