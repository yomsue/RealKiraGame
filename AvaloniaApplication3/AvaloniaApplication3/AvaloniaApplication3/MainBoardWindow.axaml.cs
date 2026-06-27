using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication3.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace AvaloniaApplication3
{
    public partial class MainBoardWindow : Window
    {
        private int _currentSuspicion = 15;

        public class HumanRow
        {
            public long ID { get; set; }
            public string ФИО { get; set; } = string.Empty;
            public int Возраст { get; set; }
            public string Должность { get; set; } = string.Empty;
            public string Греховность { get; set; } = string.Empty;
            public string Статус { get; set; } = string.Empty;
        }
        public MainBoardWindow()
        {
            InitializeComponent();

            var lblLifespan = this.FindControl<TextBlock>("LblLifespan");
            if (lblLifespan != null) lblLifespan.IsVisible = false;

            var txtTargetName = this.FindControl<TextBox>("TxtTargetName");
            if (txtTargetName != null)
            {
                txtTargetName.PropertyChanged += TxtTargetName_PropertyChanged;
            }

            var txtLogL = this.FindControl<TextBox>("TxtLogL");
            if (txtLogL != null)
            {
                txtLogL.Text = "[" + DateTime.Now.ToString("HH:mm:ss") + "] L начал секретное расследование дела Киры.\n" +
                              "[" + DateTime.Now.ToString("HH:mm:ss") + "] Национальное полицейское агентство Японии развернуло штаб опергруппы.\n" +
                              "[" + DateTime.Now.ToString("HH:mm:ss") + "] Специальные агенты Интерпола ведут круглосуточную слежку за подозреваемыми.";
            }

            RefreshGrid();
            RefreshAgentsGrid();

            this.Opened += MainBoardWindow_Opened;
        }
        private async void MainBoardWindow_Opened(object? sender, EventArgs e)
        {
            var messageBox = TupleWindow.Create(
                "ДОСТУП УСТАНОВЛЕН — СИСТЕМА УЧЕТА СУДЕБ",
                "Вы успешно синхронизировали цифровой интерфейс и вошли в сеть как полноправный Владелец Тетради Смерти.\n\n" +
                "• Ваше имя в кодовой базе: Кира\n" +
                "• Текущий статус: Вершитель правосудия и Бог Нового Мира\n" +
                "• Официальная легенда: Обычный студент университета, сын офицера полиции\n\n" +
                "Ваша главная цель — очистить этот погрязший в преступности мир от скверны, карая грешников с высоким уровнем кармического загрязнения. " +
                "Однако помните, что против вас выступает лучший детектив планеты — L, а также вся объединенная опергруппа Интерпола.\n\n" +
                "Каждое нестандартное убийство оставляет улики. Если уровень подозрения штаба L достигнет критической отметки в 100%, ваша личность будет окончательно раскрыта!"
            );
            await messageBox.ShowDialog(this);
        }

        private void BtnSearch_Click(object? sender, RoutedEventArgs e)
        {
            var txtSearchId = this.FindControl<TextBox>("TxtSearchId")?.Text;
            var txtTargetName = this.FindControl<TextBox>("TxtTargetName");
            var lblStatus = this.FindControl<TextBlock>("LblStatus");

            if (txtTargetName == null || lblStatus == null) return;
            lblStatus.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(txtSearchId) || !long.TryParse(txtSearchId, out long id))
            {
                lblStatus.Foreground = Avalonia.Media.Brushes.Red;
                lblStatus.Text = "Введите корректный числовой ID.";
                return;
            }

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();
                string query = "SELECT full_name FROM Humans WHERE id = @id LIMIT 1";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    var name = command.ExecuteScalar();

                    if (name != null)
                    {
                        txtTargetName.Text = name.ToString();
                        lblStatus.Foreground = Avalonia.Media.Brushes.Green;
                        lblStatus.Text = $"Цель с ID {id} успешно найдена.";
                    }
                    else
                    {
                        lblStatus.Foreground = Avalonia.Media.Brushes.Red;
                        lblStatus.Text = "Цель с таким ID не найдена в мире.";
                    }
                }
            }
        }
        private void TxtTargetName_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Text")
            {
                var txtTargetName = sender as TextBox;
                var lblLifespan = this.FindControl<TextBlock>("LblLifespan");
                if (txtTargetName == null || lblLifespan == null) return;

                string currentInput = txtTargetName.Text ?? "";

                if (string.IsNullOrWhiteSpace(currentInput) || currentInput.Length < 3)
                {
                    lblLifespan.IsVisible = false;
                    return;
                }

                using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT status, sin_percentage FROM Humans WHERE full_name LIKE @name LIMIT 1";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", $"%{currentInput.Trim()}%");
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader.GetString(0);
                                int sin = reader.GetInt32(1);

                                if (status == "Жив")
                                {
                                    Random rand = new Random();
                                    int randomDays = rand.Next(100, 15000);
                                    lblLifespan.Text = $"Остаток жизни цели: {randomDays} дней | Уровень грехов: {sin}%";
                                    lblLifespan.IsVisible = true;
                                }
                                else
                                {
                                    lblLifespan.IsVisible = false;
                                }
                            }
                            else
                            {
                                lblLifespan.IsVisible = false;
                            }
                        }
                    }
                }
            }
        }
        private async void BtnExecute_Click(object? sender, RoutedEventArgs e)
        {
            var txtTargetName = this.FindControl<TextBox>("TxtTargetName")?.Text;
            var cmbCause = this.FindControl<ComboBox>("CmbCause");
            var txtDetails = this.FindControl<TextBox>("TxtDetails")?.Text;
            var lblStatus = this.FindControl<TextBlock>("LblStatus");
            var lblLifespan = this.FindControl<TextBlock>("LblLifespan");
            var txtLogL = this.FindControl<TextBox>("TxtLogL");

            if (lblStatus == null) return;
            lblStatus.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(txtTargetName))
            {
                lblStatus.Foreground = Avalonia.Media.Brushes.Red;
                lblStatus.Text = "Ошибка: Имя цели не может быть пустым.";
                return;
            }

            string cause = (cmbCause?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Сердечный приступ";

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();

                string findQuery = "SELECT id, status, occupation FROM Humans WHERE full_name LIKE @name LIMIT 1";
                long humanId = -1;
                string currentStatus = "";
                string occupation = "";

                using (var findCmd = new SqliteCommand(findQuery, connection))
                {
                    findCmd.Parameters.AddWithValue("@name", $"%{txtTargetName.Trim()}%");
                    using (var reader = findCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            humanId = reader.GetInt64(0);
                            currentStatus = reader.GetString(1);
                            occupation = reader.GetString(2);
                        }
                    }
                }

                if (humanId == -1)
                {
                    lblStatus.Foreground = Avalonia.Media.Brushes.Red;
                    lblStatus.Text = "Этого человека нет в текущем мире.";
                    return;
                }

                if (currentStatus == "Mертв")
                {
                    lblStatus.Foreground = Avalonia.Media.Brushes.Red;
                    lblStatus.Text = "Этот человек уже мертв.";
                    return;
                }

                string updateQuery = "UPDATE Humans SET status = 'Мертв' WHERE id = @id";
                using (var updateCmd = new SqliteCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@id", humanId);
                    updateCmd.ExecuteNonQuery();
                }

                string insertQuery = @"
                    INSERT INTO DeathNoteEntries (user_id, human_id, cause_of_death, death_details, execution_time, is_executed)
                    VALUES (1, @humanId, @cause, @details, @time, 1)";

                using (var insertCmd = new SqliteCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@humanId", humanId);
                    insertCmd.Parameters.AddWithValue("@cause", cause);
                    insertCmd.Parameters.AddWithValue("@details", txtDetails ?? "");
                    insertCmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    insertCmd.ExecuteNonQuery();
                }
                bool isAgent = occupation.Contains("штаб") || occupation.Contains("опергрупп") || occupation.Contains("полици") || occupation.Contains("аналитик");

                if (isAgent)
                {
                    _currentSuspicion = Math.Min(100, _currentSuspicion + 30);
                    if (txtLogL != null)
                    {
                        txtLogL.Text += "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] ВНИМАНИЕ! Ликвидирован член опергруппы: " + txtTargetName + "!\n" +
                                       "[" + DateTime.Now.ToString("HH:mm:ss") + "] L объявляет чрезвычайное положение. Подозрение резко возросло!";
                    }
                }
                else if (cause != "Сердечный приступ")
                {
                    _currentSuspicion = Math.Min(100, _currentSuspicion + 15);
                    if (txtLogL != null)
                    {
                        txtLogL.Text += "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] L зафиксировал подозрительную смерть (" + cause + "). Сбор улик активирован.";
                    }
                }
                else
                {
                    if (txtLogL != null)
                    {
                        txtLogL.Text += "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] Очередной сердечный приступ. L проверяет медицинские карты.";
                    }
                }

                var progSuspicion = this.FindControl<ProgressBar>("ProgSuspicion");
                if (progSuspicion != null) progSuspicion.Value = _currentSuspicion;

                if (_currentSuspicion >= 100)
                {
                    lblStatus.Foreground = Avalonia.Media.Brushes.Red;
                    lblStatus.Text = "КРИТИЧЕСКАЯ УГРОЗА: L раскрыл вашу личность!";

                    var btnExecute = this.FindControl<Button>("BtnExecute");
                    if (btnExecute != null) btnExecute.IsEnabled = false;

                    var endBox = TupleWindow.Create(
                        "КОНЕЦ ИГРЫ — РАСКРЫТИЕ ЛИЧНОСТИ КИРЫ",
                        "Раздается оглушительный вой сирен. Здание полностью блокировано спецназом Интерпола.\n\n" +
                        "В комнату врываются вооруженные оперативники во главе с гениальным детективом L. Вы загнаны в угол, у вас изымают Тетрадь Смерти и заковывают в наручники. Суд приговаривает вас к пожизненному одиночному заключению в подземной тюрьме строгого режима без права на помилование...\n\n" +
                        "Проходит несколько дней. Внезапно в тусклом свете одиночной камеры появляется зловещий силуэт Бога Смерти. Рюк разочарованно смотрит на вас через железную решетку и тихо говорит:\n" +
                        "«Было весело, Лайт. Мы знатно развлеклись и устроили настоящий хаос в мире людей. Но если тебя засадили в эту нору до конца твоих дней, я просто сойду с ума от скуки, дожидаясь, пока ты сгниешь здесь в одиночестве.\n\n" +
                        "Правила есть правила. Пожалуй, наша игра подошла к финалу...»\n\n" +
                        "Рюк с жуткой ухмылкой открывает свою личную Тетрадь Смерти, берет ручку и уверенно выводит аккуратным почерком: «Лайт Ягами». \n\n" +
                        "Через 40 секунд ваше сердце бешено сжимается. Вы падаете на холодный бетонный пол камеры от внезапного и неизбежного сердечного приступа. Тьма застилает глаза. Система отключена."
                    );
                    await endBox.ShowDialog(this);
                    this.Close();
                }
                else
                {
                    lblStatus.Foreground = Avalonia.Media.Brushes.Green;
                    lblStatus.Text = "Приговор успешно приведен в исполнение.";
                }

                if (lblLifespan != null) lblLifespan.IsVisible = false;

                RefreshGrid();
                RefreshAgentsGrid();
            }
        }

        private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
        {
            RefreshGrid();
            RefreshAgentsGrid();
        }
        private void RefreshGrid()
        {
            var gridHumans = this.FindControl<DataGrid>("GridHumans");
            if (gridHumans == null) return;

            var list = new List<HumanRow>();

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();
                string query = "SELECT id, full_name, age, occupation, sin_percentage, status FROM Humans";
                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new HumanRow
                            {
                                ID = reader.GetInt64(0),
                                ФИО = reader.GetString(1),
                                Возраст = reader.GetInt32(2),
                                Должность = reader.GetString(3),
                                Греховность = reader.GetInt32(4).ToString() + "%",
                                Статус = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            gridHumans.ItemsSource = list;
        }

        private void RefreshAgentsGrid()
        {
            var gridAgents = this.FindControl<DataGrid>("GridAgents");
            if (gridAgents == null) return;

            var list = new List<HumanRow>();

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();
                string query = "SELECT id, full_name, age, occupation, sin_percentage, status FROM Humans WHERE occupation LIKE '%штаб%' OR occupation LIKE '%опергрупп%' OR occupation LIKE '%аналитик%' OR occupation LIKE '%полици%'";
                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new HumanRow
                            {
                                ID = reader.GetInt64(0),
                                ФИО = reader.GetString(1),
                                Возраст = reader.GetInt32(2),
                                Должность = reader.GetString(3),
                                Греховность = reader.GetInt32(4).ToString() + "%",
                                Статус = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            gridAgents.ItemsSource = list;
        }

        private void BtnCreateHuman_Click(object? sender, RoutedEventArgs e)
        {
            string[] firstNames = { "Даниил", "Александр", "Иван", "Сергей", "Дмитрий", "Светлана", "Анна" };
            string[] lastNames = { "Фарунцев", "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов" };
            string[] jobs = { "Инженер", "Врач-хирург", "Студент университета", "Программист", "Бухгалтер" };

            Random rand = new Random();
            string fullName = $"{lastNames[rand.Next(lastNames.Length)]} {firstNames[rand.Next(firstNames.Length)]}";
            int age = rand.Next(18, 70);
            string job = jobs[rand.Next(jobs.Length)];
            int sin = rand.Next(10, 95);

            using (var connection = new SqliteConnection(DatabaseManager.ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Humans (full_name, age, occupation, sin_percentage) VALUES (@name, @age, @job, @sin)";
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", fullName);
                    command.Parameters.AddWithValue("@age", age);
                    command.Parameters.AddWithValue("@job", job);
                    command.Parameters.AddWithValue("@sin", sin);
                    command.ExecuteNonQuery();
                }
            }

            RefreshGrid();
        }
    }

    public static class TupleWindow
    {
        public static Window Create(string title, string text)
        {
            var win = new Window
            {
                Title = title,
                Width = 600,
                Height = 420,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Avalonia.Media.Brushes.Black
            };

            var mainGrid = new Grid { RowDefinitions = new RowDefinitions("*, Auto"), Margin = new Avalonia.Thickness(20) };

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Margin = new Avalonia.Thickness(0, 0, 0, 15) };
            Grid.SetRow(scroll, 0); 
            var panel = new StackPanel { Spacing = 15 };
            panel.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = Avalonia.Media.FontWeight.Bold, Foreground = Avalonia.Media.Brushes.Red, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center });
            panel.Children.Add(new TextBlock { Text = text, FontSize = 12, Foreground = Avalonia.Media.Brushes.White, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

            scroll.Content = panel;
            mainGrid.Children.Add(scroll);

            var btn = new Button { Content = "ПРИНЯТЬ СУДЬБУ", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Avalonia.Thickness(25, 10), Background = Avalonia.Media.Brushes.DarkRed, Foreground = Avalonia.Media.Brushes.White, FontWeight = Avalonia.Media.FontWeight.Bold };
            Grid.SetRow(btn, 1);
            btn.Click += (s, e) => win.Close();
            mainGrid.Children.Add(btn);

            win.Content = mainGrid;
            return win;
        }
    }
}
