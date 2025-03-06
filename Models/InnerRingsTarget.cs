using System.Collections.Generic;
using Avalonia;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RFD.Models
{
    public class InnerRingsTarget : INotifyPropertyChanged
    {
        private double _width;
        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _order;
        public int Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        private double _radius;
        public double Radius
        {
            get => _radius;
            set => SetProperty(ref _radius, value);
        }

        private Thickness _margin;
        public Thickness Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public InnerRingsTarget(double width, double height, int order, double radius, Thickness margin)
        {
            _width = width;
            _height = height;
            _order = order;
            _radius = radius;
            _margin = margin;
        }

        // Метод для уведомления об изменении свойства
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Метод для уведомления о изменении
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}