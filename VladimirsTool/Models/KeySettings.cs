﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using VladimirsTool.ViewModels;

namespace VladimirsTool.Models
{
    public class DateFormat
    {
        public string Format { get; set; }
        public static implicit operator string(DateFormat obj)
        {
            return obj.Format;
        }
        public static implicit operator DateFormat(string str)
        {
            return new DateFormat(str);
        }
        public DateFormat(string format)
        {
            this.Format = format;
        }
    }

    public class KeySettings : INotifyPropertyChanged, ICloneable
    {
        private bool _isDate, _isSelected;
        private ObservableCollection<DateFormat> _inputFormats = new ObservableCollection<DateFormat>();
        public static string GetDateFormatFromRU(string ruFormat)
        {
            char[] enFormat = new char[ruFormat.Length];
            for (int i = 0; i < ruFormat.Length; i++)
            {
                char ch = ruFormat[i];
                switch (ch)
                {
                    case 'Г':
                        ch = 'y';
                        break;
                    case 'М':
                        ch = 'M';
                        break;
                    case 'Д':
                        ch = 'd';
                        break;
                    case 'ч':
                        ch = 'H';
                        break;
                    case 'м':
                        ch = 'm';
                        break;
                    case 'с':
                        ch = 's';
                        break;
                }
                enFormat[i] = ch;
            }
            return new string(enFormat);
        }

        public ObservableCollection<DateFormat> InputFormats
        {
            get => _inputFormats;
            set
            {
                _inputFormats = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddInputFormat
        {
            get => new ClickCommand((obj) =>
            {
                InputFormats.Add("ДД.ММ.ГГГГ");
                OnPropertyChanged(nameof(InputFormats));
            });
        }

        public ICommand RemoveLastInputFormat
        {
            get => new ClickCommand((obj) =>
            {
                if (InputFormats.Count == 0) return;
                InputFormats.RemoveAt(InputFormats.Count-1);
                OnPropertyChanged(nameof(InputFormats));
            });
        }

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
        public string OutDateFormat { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if(!_isSelected)
                    IsDate = false;
                OnPropertyChanged(nameof(DateCheckBoxVisible));
            }
        }
        public Visibility DateFormatVisible => IsDate ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DateCheckBoxVisible => IsSelected ? Visibility.Visible : Visibility.Hidden;

        public KeySettings(string header, bool isDate = false, string inDateFormat = null, string outDateFormat = null)
        {
            Header = header;
            IsDate = isDate;
            OutDateFormat = outDateFormat ?? "ДД.ММ.ГГГГ";
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
