using System.Windows;
using PasswordManager.Views;

namespace PasswordManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result != true || string.IsNullOrEmpty(loginWindow.Passphrase))
            {
                Shutdown();
                return;
            }

            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var mainWindow = new MainWindow(loginWindow.Passphrase);
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
