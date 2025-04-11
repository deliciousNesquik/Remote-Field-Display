using ReactiveUI;
using RFD.Interfaces;

namespace RFD.Models;

/// <summary>
///     Представляет статус состояния компонента буровой установки.
/// </summary>
public class StatusBox : ViewModelBase
{
    private string _header;
    private bool _status;

    /// <summary>
    ///     Создает экземпляр <see cref="StatusBox" /> с заданным названием и состоянием.
    /// </summary>
    /// <param name="header">Название компонента буровой установки.</param>
    /// <param name="status">Состояние компонента (по умолчанию false — неисправность).</param>
    public StatusBox(string header = "", bool status = false)
    {
        _header = header;
        _status = status;
    }

    /// <summary>
    ///     Название компонента буровой установки.
    /// </summary>
    public string Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }

    /// <summary>
    ///     Состояние компонента.
    /// </summary>
    /// <remarks>
    ///     <para>true — компонент работает нормально.</para>
    ///     <para>false — есть проблемы или неисправность.</para>
    /// </remarks>
    public bool Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }
}