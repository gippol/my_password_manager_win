using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PasswordManager.Models;
using PasswordManager.Services;

namespace PasswordManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly string _passphrase;
        private readonly CsvFileService _csvService;

        private ObservableCollection<PasswordEntry> _entries;
        private PasswordEntry _selectedEntry;
        private string _currentFileName;
        private bool _isLoading;

        public ObservableCollection<PasswordEntry> Entries
        {
            get => _entries;
            set { _entries = value; OnPropertyChanged(nameof(Entries)); }
        }

        public PasswordEntry SelectedEntry
        {
            get => _selectedEntry;
            set { _selectedEntry = value; OnPropertyChanged(nameof(SelectedEntry)); }
        }

        public string CurrentFileName
        {
            get => _currentFileName;
            set { _currentFileName = value; OnPropertyChanged(nameof(CurrentFileName)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        public MainViewModel(string passphrase)
        {
            _passphrase = passphrase;
            _csvService = new CsvFileService(new CryptoService());
            _entries = new ObservableCollection<PasswordEntry>();

            AddCommand = new RelayCommand(ExecuteAdd);
            DeleteCommand = new RelayCommand(ExecuteDelete, () => SelectedEntry != null);
            SaveCommand = new RelayCommand(ExecuteSave);

            _ = LoadLatestFileAsync();
        }

        private async Task LoadLatestFileAsync()
        {
            var latestFile = _csvService.FindLatestFile();
            if (latestFile == null)
            {
                CurrentFileName = "（ファイルなし）";
                return;
            }

            IsLoading = true;
            try
            {
                var loaded = await Task.Run(() => _csvService.Load(latestFile, _passphrase));
                Entries = new ObservableCollection<PasswordEntry>(loaded);
                CurrentFileName = Path.GetFileName(latestFile);
            }
            catch
            {
                MessageBox.Show(
                    "ファイルの読み込みに失敗しました。パスフレーズが正しくない可能性があります。",
                    "読み込みエラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                CurrentFileName = "（読み込み失敗）";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteAdd()
        {
            var entry = new PasswordEntry { Site = "", Username = "", Password = "", Notes = "" };
            Entries.Add(entry);
            SelectedEntry = entry;
        }

        private void ExecuteDelete()
        {
            if (SelectedEntry == null) return;
            Entries.Remove(SelectedEntry);
            SelectedEntry = null;
        }

        private async void ExecuteSave()
        {
            var snapshot = Entries.ToList();
            IsLoading = true;
            try
            {
                var savedPath = await Task.Run(() => _csvService.Save(snapshot, _passphrase));
                CurrentFileName = Path.GetFileName(savedPath);
                MessageBox.Show(
                    $"保存しました:\n{Path.GetFileName(savedPath)}",
                    "保存完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"保存に失敗しました:\n{ex.Message}",
                    "保存エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
