using System.Windows;
using PasswordManager.ViewModels;

namespace PasswordManager.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;

        public string Passphrase => _vm.Passphrase;

        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            DataContext = _vm;

            _vm.CloseRequested += (_, success) =>
            {
                DialogResult = success;
                Close();
            };
        }

        private void PassphraseBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _vm.Passphrase = PassphraseBox.Password;
        }
    }
}
