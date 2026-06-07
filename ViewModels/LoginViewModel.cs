using System.Windows.Input;
using PasswordManager.ViewModels;

namespace PasswordManager.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _passphrase;

        public string Passphrase
        {
            get => _passphrase;
            set
            {
                _passphrase = value;
                OnPropertyChanged(nameof(Passphrase));
            }
        }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public bool? DialogResult { get; private set; }

        public LoginViewModel()
        {
            OkCommand = new RelayCommand(ExecuteOk, () => !string.IsNullOrEmpty(_passphrase));
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private void ExecuteOk()
        {
            DialogResult = true;
            CloseRequested?.Invoke(this, true);
        }

        private void ExecuteCancel()
        {
            DialogResult = false;
            CloseRequested?.Invoke(this, false);
        }

        public event System.EventHandler<bool> CloseRequested;
    }
}
