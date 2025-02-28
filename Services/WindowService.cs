using Avalonia.Controls;
using RFD.Interfaces;

namespace RFD.Services;

public class WindowService : IWindowService
{
    public void OpenWindow<T>(T content, string title, int width = 500, int height = 400) where T : Control
    {
        var window = new Window
        {
            Title = title,
            Content = content,
            Width = width,
            Height = height
        };

        window.Show();
    }
}