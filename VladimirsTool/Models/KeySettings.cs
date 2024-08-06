using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VladimirsTool.Models
{
    public class KeySettings : INotifyPropertyChanged, ICloneable
    {
        private bool _isDate;

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
        public string DateFormat { get; set; }
        public bool IsSelected { get; set; }
        public Visibility DateFormatVisible => IsDate ? Visibility.Visible : Visibility.Hidden;

        public KeySettings(string header, bool isDate = false, string dateFormat = null)
        {
            Header = header;
            IsDate = isDate;
            DateFormat = dateFormat ?? "dd.MM.yyyy";
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
