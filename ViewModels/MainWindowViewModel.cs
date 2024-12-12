using System.Collections.ObjectModel;
using RFD.Models;

namespace RFD.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<InfoBlock> InfoBlockList { get; }

        public MainWindowViewModel() 
        {
            this.InfoBlockList =
            [
                new ("Высота блока", "-", "м"),
                new ("Глубина долота", "-", "м"),
                new ("Текущий забой", "-", "м"),
                new ("Rop средний", "-", "м/ч"),
                new ("Расстояние до забоя", "-", "м"),
                new ("Зенит", "-", "°"),
                new ("Азимут", "-", "°"),
                new ("TVD", "-", "м"),
            ];
        }
    }
}
