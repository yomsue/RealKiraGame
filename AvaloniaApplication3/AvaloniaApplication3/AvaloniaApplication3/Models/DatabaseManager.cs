using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AvaloniaApplication3.Models
{
    internal static class DatabaseManager
    {
        private const string DbName = "deathnote.db";
        public static string ConnectionString => $"Data Source={DbName}";

        public static void InitializeDatabase()
        {
            if (File.Exists(DbName)) return;

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL UNIQUE,
                        password_hash TEXT NOT NULL,
                        lifespan_days INTEGER DEFAULT 10000
                    );

                    CREATE TABLE IF NOT EXISTS Humans (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        full_name TEXT NOT NULL,
                        photo_path TEXT,
                        status TEXT DEFAULT 'Жив',
                        age INTEGER NOT NULL,
                        occupation TEXT DEFAULT 'Нет данных',
                        sin_percentage INTEGER DEFAULT 0
                    );

                    CREATE TABLE IF NOT EXISTS DeathNoteEntries (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER,
                        human_id INTEGER,
                        cause_of_death TEXT DEFAULT 'Сердечный приступ',
                        death_details TEXT,
                        execution_time TEXT NOT NULL,
                        is_executed INTEGER DEFAULT 0,
                        FOREIGN KEY(user_id) REFERENCES Users(id),
                        FOREIGN KEY(human_id) REFERENCES Humans(id)
                    );

                    CREATE TABLE IF NOT EXISTS Investigations (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        investigator_name TEXT NOT NULL,
                        suspicion_level INTEGER DEFAULT 0,
                        case_status TEXT DEFAULT 'Активно'
                    );

                    INSERT INTO Users (username, password_hash, lifespan_days) 
                    VALUES ('Рюк', '1234', 99999);

                    INSERT INTO Humans (full_name, age, occupation, sin_percentage) VALUES ('Лайт Ягами', 17, 'Лучший студент Японии', 45);
                    INSERT INTO Humans (full_name, age, occupation, sin_percentage) VALUES ('Такуо Сибуима', 26, 'Торговец запрещенными веществами', 92);
                ";
                command.ExecuteNonQuery();

                string[] firstNames = {
                    "Даниил", "Александр", "Иван", "Сергей", "Дмитрий", "Андрей", "Алексей", "Михаил", "Николай", "Владимир",
                    "Артем", "Егор", "Никита", "Максим", "Олег", "Павел", "Антон", "Илья", "Роман", "Денис",
                    "Анна", "Светлана", "Елена", "Мария", "Ольга", "Наталья", "Татьяна", "Ирина", "Екатерина", "Юлия"
                };

                string[] lastNames = {
                    "Фарунцев", "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Попов", "Васильев", "Соколов", "Новиков",
                    "Морозов", "Волков", "Федоров", "Лебедев", "Козлов", "Степанов", "Николаев", "Орлов", "Андреев",
                    "Павлов", "Макаров", "Зайцев", "Голубев", "Виноградов", "Богданов", "Воробьев", "Киселев", "Сорокин", "Фролов"
                };

                string[] cleanJobs = {
                    "Инженер", "Врач-хирург", "Студент университета", "Программист", "Бухгалтер", "Юрист", "Преподаватель",
                    "Менеджер", "Повар", "Водитель", "Автомеханик", "Архитектор", "Журналист", "Дизайнер"
                };

                string[] darkJobs = {
                    "Профессиональный карманник", "Хакер-вымогатель", "Фальшивомонетчик", "Контрабандист оружия",
                    "Угонщик элитных авто", "Организатор подпольного казино", "Лидер уличной банды", "Черный копатель",
                    "Аферист в сфере недвижимости", "Скупщик краденого", "Грабитель ювелирных лавок"
                };

                Random rand = new Random();

                for (int i = 0; i < 198; i++)
                {
                    string fullName = $"{lastNames[rand.Next(lastNames.Length)]} {firstNames[rand.Next(firstNames.Length)]}";
                    int age = rand.Next(18, 65);
                    string job;
                    int sin;

                    if (rand.Next(100) < 35)
                    {
                        job = darkJobs[rand.Next(darkJobs.Length)];
                        sin = rand.Next(75, 99);
                    }
                    else
                    {
                        job = cleanJobs[rand.Next(cleanJobs.Length)];
                        sin = rand.Next(5, 50);
                    }

                    var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO Humans (full_name, age, occupation, sin_percentage) VALUES (@name, @age, @job, @sin)";
                    insertCmd.Parameters.AddWithValue("@name", fullName);
                    insertCmd.Parameters.AddWithValue("@age", age);
                    insertCmd.Parameters.AddWithValue("@job", job);
                    insertCmd.Parameters.AddWithValue("@sin", sin);
                    insertCmd.ExecuteNonQuery();
                }

                string[] agents = { "Соитиро Ягами", "Тота Мацуда", "Кандзо Моги", "Сюити Аизава", "Хидеки Иде" };
                string[] agentRoles = { "Глава опергруппы расследования", "Детектив штаба L", "Спецагент полиции", "Старший офицер штаба L", "Следственный аналитик" };

                for (int i = 0; i < agents.Length; i++)
                {
                    var agentCmd = connection.CreateCommand();
                    agentCmd.CommandText = "INSERT INTO Humans (full_name, age, occupation, sin_percentage) VALUES (@name, @age, @job, @sin)";
                    agentCmd.Parameters.AddWithValue("@name", agents[i]);
                    agentCmd.Parameters.AddWithValue("@age", rand.Next(30, 55));
                    agentCmd.Parameters.AddWithValue("@job", agentRoles[i]);
                    agentCmd.Parameters.AddWithValue("@sin", rand.Next(5, 15));
                    agentCmd.ExecuteNonQuery();
                }
            }
        }
    }
}
