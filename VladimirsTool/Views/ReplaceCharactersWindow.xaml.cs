using System.Windows;
using VladimirsTool.ViewModels;

namespace VladimirsTool.Views
{
    public partial class ReplaceCharactersWindow : Window
    {
        private bool _isApplied = false;
        public bool IsApplied => _isApplied;
        public ReplaceCharactersWindow()
        {
            InitializeComponent();
            ((ReplaceCharactersViewModel)DataContext).OnApplyButton += () =>
            {
                _isApplied = true;
                this.Close();
            };
        }
    }
}
