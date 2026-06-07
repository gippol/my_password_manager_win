using System.ComponentModel;

namespace PasswordManager.Models
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        private string _site;
        private string _username;
        private string _password;
        private string _notes;

        public string Site
        {
            get => _site;
            set { _site = value; OnPropertyChanged(nameof(Site)); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(nameof(Notes)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
