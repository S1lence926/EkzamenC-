using System;
using System.Data.SqlClient;

namespace LibraryApp
{
    class Program
    {
        
        private const string ConnectionString =
            "Server=localhost;Database=LibraryDB;Integrated Security=True;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Библиотека — ADO.NET";

            
            if (!TestConnection())
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            InitializeDatabase();

            bool running = true;
            while (running)
            {
                ShowMenu();
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        ShowAllBooks();
                        break;
                    case "2":
                        AddBook();
                        break;
                    case "3":
                        SearchBook();
                        break;
                    case "4":
                        DeleteBook();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("\nДо свидания!");
                        break;
                    default:
                        PrintError("Неверный выбор. Введите число от 0 до 4.");
                        break;
                }

                if (running)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("БИБЛИОТЕКА — ADO.NET");
            Console.WriteLine("1 - Показать все книги");
            Console.WriteLine("2 - Добавить книгу");
            Console.WriteLine("3 - Найти книгу по названию");
            Console.WriteLine("4 - Удалить книгу по ID");
            Console.WriteLine("0 - Выход");
            Console.Write("\nВаш выбор: ");
        }

        static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    Console.WriteLine(" Подключение к базе данных успешно установлено.");
                    return true;
                }
            }
            catch (SqlException ex)
            {
                PrintError($"Ошибка подключения к БД: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                PrintError($"Неожиданная ошибка: {ex.Message}");
                return false;
            }
        }


        static void InitializeDatabase()
        {
            string sql = @"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = 'Books'
                )
                BEGIN
                    CREATE TABLE Books (
                        Id          INT PRIMARY KEY IDENTITY(1,1),
                        Title       NVARCHAR(100),
                        Author      NVARCHAR(100),
                        PublishYear INT,
                        Quantity    INT
                    );
                END";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                PrintError($"Ошибка инициализации БД: {ex.Message}");
            }
        }


        static void ShowAllBooks()
        {
            Console.Clear();
            Console.WriteLine("СПИСОК КНИГ\n");

            string sql = "SELECT Id, Title, Author, PublishYear, Quantity FROM Books ORDER BY Id";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Console.WriteLine("  В базе данных нет книг.");
                            return;
                        }

                        Console.WriteLine($"{"ID",-5} {"Название",-30} {"Автор",-25} {"Год",-6} {"Кол-во"}");
                        Console.WriteLine(new string('─', 76));

                        while (reader.Read())
                        {
                            int    id       = reader.GetInt32(0);
                            string title    = reader.GetString(1);
                            string author   = reader.GetString(2);
                            int    year     = reader.GetInt32(3);
                            int    quantity = reader.GetInt32(4);

                            string shortTitle  = title.Length  > 28 ? title.Substring(0, 28)  + ".." : title;
                            string shortAuthor = author.Length > 23 ? author.Substring(0, 23) + ".." : author;

                            Console.WriteLine($"{id,-5} {shortTitle,-30} {shortAuthor,-25} {year,-6} {quantity}");
                        }

                        Console.WriteLine(new string('─', 76));
                    }
                }
            }
            catch (SqlException ex)
            {
                PrintError($"Ошибка при получении данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                PrintError($"Неожиданная ошибка: {ex.Message}");
            }
        }


        static void AddBook()
        {
            Console.Clear();
            Console.WriteLine("ДОБАВИТЬ КНИГУ\n");

            try
            {
                string title = ReadNonEmpty("Название книги : ");
                string author = ReadNonEmpty("Автор          : ");
                int year = ReadInt("Год издания    : ", 1, 2100);
                int quantity = ReadInt("Количество     : ", 0, 10000);

                string sql = @"
                    INSERT INTO Books (Title, Author, PublishYear, Quantity)
                    VALUES (@Title, @Author, @Year, @Quantity)";

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Title",    title);
                        cmd.Parameters.AddWithValue("@Author",   author);
                        cmd.Parameters.AddWithValue("@Year",     year);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                            PrintSuccess("Книга успешно добавлена!");
                        else
                            PrintError("Не удалось добавить книгу.");
                    }
                }
            }
            catch (SqlException ex)
            {
                PrintError($"Ошибка при добавлении книги: {ex.Message}");
            }
            catch (Exception ex)
            {
                PrintError($"Неожиданная ошибка: {ex.Message}");
            }
        }

      
        static void SearchBook()
        {
            Console.Clear();
            Console.WriteLine("ПОИСК КНИГИ\n");

            Console.Write("Введите название (или часть названия): ");
            string keyword = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                PrintError("Поисковый запрос не может быть пустым.");
                return;
            }

            string sql = @"
                SELECT Id, Title, Author, PublishYear, Quantity
                FROM Books
                WHERE Title LIKE @Keyword
                ORDER BY Title";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            Console.WriteLine();

                            if (!reader.HasRows)
                            {
                                Console.WriteLine($"  Книги с названием «{keyword}» не найдены.");
                                return;
                            }

                            Console.WriteLine($"{"ID",-5} {"Название",-30} {"Автор",-25} {"Год",-6} {"Кол-во"}");
                            Console.WriteLine(new string('─', 76));

                            int count = 0;
                            while (reader.Read())
                            {
                                int    id       = reader.GetInt32(0);
                                string title    = reader.GetString(1);
                                string author   = reader.GetString(2);
                                int    year     = reader.GetInt32(3);
                                int    quantity = reader.GetInt32(4);

                                string shortTitle  = title.Length  > 28 ? title.Substring(0, 28)  + ".." : title;
                                string shortAuthor = author.Length > 23 ? author.Substring(0, 23) + ".." : author;

                                Console.WriteLine($"{id,-5} {shortTitle,-30} {shortAuthor,-25} {year,-6} {quantity}");
                                count++;
                            }

                            Console.WriteLine(new string('─', 76));
                            Console.WriteLine($"\n  Найдено записей: {count}");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                PrintError($"Ошибка при поиске: {ex.Message}");
            }
            catch (Exception ex)
            {
                PrintError($"Неожиданная ошибка: {ex.Message}");
            }
        }

        
        static void DeleteBook()
        {
            Console.Clear();
            Console.WriteLine("УДАЛИТЬ КНИГУ\n");

            ShowAllBooks();
            Console.WriteLine();

            int id;
            try
            {
                id = ReadInt("Введите ID книги для удаления: ", 1, int.MaxValue);
            }
            catch (Exception)
            {
                PrintError("Некорректный ID.");
                return;
            }

            string selectSql = "SELECT Title, Author FROM Books WHERE Id = @Id";
            string deleteSql = "DELETE FROM Books WHERE Id = @Id";

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    string title = null, author = null;
                    using (SqlCommand selectCmd = new SqlCommand(selectSql, conn))
                    {
                        selectCmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                PrintError($"Книга с ID = {id} не найдена.");
                                return;
                            }
                            reader.Read();
                            title  = reader.GetString(0);
                            author = reader.GetString(1);
                        }
                    }

                    Console.WriteLine($"\n  Будет удалена книга: «{title}» — {author}");
                    Console.Write("  Подтвердите удаление (y/n): ");
                    string confirm = Console.ReadLine()?.Trim().ToLower();

                    if (confirm != "y" && confirm != "д")
                    {
                        Console.WriteLine("  Удаление отменено.");
                        return;
                    }

                    using (SqlCommand deleteCmd = new SqlCommand(deleteSql, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@Id", id);
                        int rows = deleteCmd.ExecuteNonQuery();
                        if (rows > 0)
                            PrintSuccess($"Книга «{title}» успешно удалена.");
                        else
                            PrintError("Не удалось удалить книгу.");
                    }
                }
            }
            catch (SqlException ex)
            {
                PrintError($"Ошибка при удалении: {ex.Message}");
            }
            catch (Exception ex)
            {
                PrintError($"Неожиданная ошибка: {ex.Message}");
            }
        }

            static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(value))
                    return value;
                PrintError("Поле не может быть пустым. Повторите ввод.");
            }
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;
                PrintError($"Введите целое число от {min} до {max}.");
            }
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ✖ {message}");
            Console.ResetColor();
        }

        static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✔ {message}");
            Console.ResetColor();
        }
    }
}
