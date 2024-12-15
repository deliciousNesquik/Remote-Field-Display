using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using RFD.Models;

namespace RFD.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<InfoBlock> InfoBlockList { get; private set; }
        public ObservableCollection<InfoStatus> InfoStatusList { get; private set; }
        
        public double MagneticDeclination { get; private set; } 
        public double ToolfaceOffset { get; private set; } 

        public MainWindowViewModel() 
        {
            InfoBlockList =
            [
                new ("Высота блока", "-", "м"),
                new ("Глубина долота", "-", "м"),
                new ("Текущий забой", "-", "м"),
                new ("TVD", "-", "м"),
                new ("Расстояние до забоя", "-", "м"),
                new ("Rop средний", "-", "м/ч"),
                new ("Зенит", "-", "°"),
                new ("Азимут", "-", "°"),
            ];

            InfoStatusList =
            [
                new ("Клинья", ""),
                new ("Насос", ""),
                new ("Забой", ""),
            ];

            MagneticDeclination = 0.00;
            ToolfaceOffset = 0.00;
        }
    }
}
