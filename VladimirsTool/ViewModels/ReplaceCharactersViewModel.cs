using System.Collections.ObjectModel;
using System.Windows.Input;
using VladimirsTool.Models;

namespace VladimirsTool.ViewModels
{
    public class ReplaceCharactersViewModel : BaseVM
    {
        private ObservableCollection<ReplacedValue> _values = new ObservableCollection<ReplacedValue>();
        public delegate void ApplyHandler();
        public event ApplyHandler OnApplyButton;
        public bool LeftOrigin { get; set; } = true;

        public ObservableCollection<ReplacedValue> Values
        {
            get => _values;
            set
            {
                _values = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddReplacingUnit
        {
            get => new ClickCommand((obj) =>
            {
                Values.Add(new ReplacedValue());
            });
        }

        public ICommand RemoveReplacingUnit
        {
            get => new ClickCommand((obj) =>
            {
                Values.RemoveAt(Values.Count - 1);
            });
        }

        public ICommand Apply
        {
            get => new ClickCommand((obj) =>
            {
                OnApplyButton?.Invoke();
            });
        }
    }
}
