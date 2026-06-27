using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication3.Models;
using Microsoft.Data.Sqlite;

namespace AvaloniaApplication3
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TxtUser_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                var passwordBox = this.FindControl<TextBox>("TxtPass");
                if (passwordBox != null)
                {
                    passwordBox.Focus();
                }
            }
        }

        private void BtnLogin_Click(object? sender, RoutedEventArgs e)
        {
            var login = this.FindControl<TextBox>("TxtUser")?.Text;
            var password = this.FindControl<TextBox>("TxtPass")?.Text;
            var errorLabel = this.FindControl<TextBlock>("LblError");

            if (errorLabel == null) return;
            errorLabel.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                errorLabel.Text = "Заполните все поля ввода.";
                return;
            }

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();
                string query = "SELECT id FROM Users WHERE username = @user AND password_hash = @pass";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@user", login.Trim());
                    command.Parameters.AddWithValue("@pass", password.Trim());

                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        var mainBoard = new MainBoardWindow();
                        mainBoard.Show();
                        this.Close();
                    }
                    else
                    {
                        errorLabel.Foreground = Avalonia.Media.Brushes.Red;
                        errorLabel.Text = "Доступ отклонен. Имя или ключ не найдены.";
                    }
                }
            }
        }
    }
}
