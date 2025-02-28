using Avalonia.Controls;
namespace RFD.Interfaces;

public interface IWindowService
{
    void OpenWindow<T>(T content, string title, int width = 500, int height = 400) where T : Control;
}
