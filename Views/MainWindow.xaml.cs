using System.Windows;
using PasswordManager.ViewModels;

namespace PasswordManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(string passphrase)
        {
            InitializeComponent();
            DataContext = new MainViewModel(passphrase);
        }
    }
}
