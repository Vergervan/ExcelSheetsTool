using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VladimirsTool.Models
{
    public class KeySettings : INotifyPropertyChanged, ICloneable
    {
        private bool _isDate, _isSelected;

        public string Header { get; set; }
        public bool IsDate
        {
            get => _isDate;
            set
            {
                _isDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateFormatVisible));
            }
        }
        public string InDateFormat { get; set; }
        public string OutDateFormat { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(DateCheckBoxVisible));
            }
        }
        public Visibility DateFormatVisible => IsDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DateCheckBoxVisible => IsSelected ? Visibility.Visible : Visibility.Hidden;

        public KeySettings(string header, bool isDate = false, string inDateFormat = null, string outDateFormat = null)
        {
            Header = header;
            IsDate = isDate;
            InDateFormat = inDateFormat ?? "dd.MM.yyyy";
            OutDateFormat = outDateFormat ?? "dd.MM.yyyy";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public override string ToString()
        {
            return Header;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
