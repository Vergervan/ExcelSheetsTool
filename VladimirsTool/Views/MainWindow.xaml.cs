using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VladimirsTool.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((ViewModels.MainViewModel)DataContext)
                .FileItemDoubleClick.Execute(((ListViewItem)sender).Content);
        }
    }
}
