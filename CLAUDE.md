# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```powershell
# Build (Debug)
dotnet build PasswordManager.csproj

# Build (Release)
dotnet build PasswordManager.csproj -c Release

# Run
dotnet run --project PasswordManager.csproj
```

There are no automated tests in this project.

## Architecture

WPF desktop app targeting .NET 4.8, using MVVM pattern. No third-party frameworks — MVVM infrastructure (`ViewModelBase`, `RelayCommand`) is hand-rolled.

**Startup flow** (`App.xaml.cs`): `LoginWindow` is shown as a modal dialog first. If the user cancels, the app shuts down. On success, the passphrase is passed directly to `MainWindow`, which creates `MainViewModel` with it. The passphrase is held in memory for the entire session and used for every encrypt/decrypt operation.

**Data flow**:
- `CsvFileService` reads/writes CSV files named `passwords_<yyyyMMdd_HHmmss>.csv` in the same directory as the executable.
- On load, `FindLatestFile()` picks the most recent file by filename sort. Each save creates a new timestamped file (old files are not deleted).
- Only the `Password` field is encrypted; `Site`, `Username`, and `Notes` are stored in plaintext in the CSV.

**Encryption** (`CryptoService`): AES-256-CBC with PBKDF2-SHA256 key derivation (100,000 iterations). Each encrypted value embeds its own random salt (16 bytes) and IV (16 bytes) prepended to the ciphertext, then Base64-encoded. There is no passphrase verification step — a wrong passphrase causes decryption to throw, which `MainViewModel.LoadLatestFile()` catches and reports to the user.

**PasswordBox limitation**: WPF's `PasswordBox` does not support data binding, so `LoginWindow.xaml.cs` manually syncs `PassphraseBox.Password` → `LoginViewModel.Passphrase` via the `PasswordChanged` event.

**UI**: Japanese locale strings are used throughout (labels, error messages, status text).
