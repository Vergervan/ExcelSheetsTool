using System.Collections.Generic;
using System.Collections.ObjectModel;
using KeySettings = VladimirsTool.Models.KeySettings;

namespace VladimirsTool.ViewModels
{
    public class KeyViewModel: BaseVM
    {
        private ObservableCollection<KeySettings> _headers;

        public ObservableCollection<KeySettings> Headers
        {
            get => _headers;
            set
            {
                _headers = value;
                OnPropertyChanged();
            }
        }

        
    }
}
