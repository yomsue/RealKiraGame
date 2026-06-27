using AvaloniaApplication3.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Windows.Input;

using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand LoginCommand { get; }

        public MainWindowViewModel()
        {
            LoginCommand = new RelayCommand(TryLogin);
        }

        private void TryLogin()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все поля ввода.";
                return;
            }

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();
                string query = "SELECT id FROM Users WHERE username = @user AND password_hash = @pass";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user", Username.Trim());
                    command.Parameters.AddWithValue("@pass", Password.Trim());

                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        long userId = (long)result;
                        ErrorMessage = $"Успешный вход! ID Бога Смерти: {userId}";
                    }
                    else
                    {
                        ErrorMessage = "Доступ отклонен. Имя или ключ не найдены в системе.";
                    }
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
