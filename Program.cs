using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryApp
{
    class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int PublishYear { get; set; }
        public int Quantity { get; set; }
    }

    class Program
    {
        static List<Book> books = new List<Book>();
        static int nextId = 1;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Библиотека";

            books.Add(new Book { Id = nextId++, Title = "Мастер и Маргарита", Author = "Михаил Булгаков", PublishYear = 1967, Quantity = 5 });
            books.Add(new Book { Id = nextId++, Title = "Война и мир", Author = "Лев Толстой", PublishYear = 1869, Quantity = 3 });
            books.Add(new Book { Id = nextId++, Title = "Преступление и наказание", Author = "Фёдор Достоевский", PublishYear = 1866, Quantity = 7 });
            books.Add(new Book { Id = nextId++, Title = "1984", Author = "Джордж Оруэлл", PublishYear = 1949, Quantity = 4 });

            bool running = true;
            while (running)
            {
                ShowMenu();
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": ShowAllBooks(); break;
                    case "2": AddBook(); break;
                    case "3": SearchBook(); break;
                    case "4": DeleteBook(); break;
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
            Console.WriteLine("╔══════════════════════════════════╗");
            Console.WriteLine("║       БИБЛИОТЕКА — ADO.NET       ║");
            Console.WriteLine("╠══════════════════════════════════╣");
            Console.WriteLine("║  1 - Показать все книги          ║");
            Console.WriteLine("║  2 - Добавить книгу              ║");
            Console.WriteLine("║  3 - Найти книгу по названию     ║");
            Console.WriteLine("║  4 - Удалить книгу по ID         ║");
            Console.WriteLine("║  0 - Выход                       ║");
            Console.WriteLine("╚══════════════════════════════════╝");
            Console.Write("\nВаш выбор: ");
        }


        static void ShowAllBooks()
        {
            Console.Clear();
            Console.WriteLine("═══════════════ СПИСОК КНИГ ═══════════════\n");

            try
            {
                if (books.Count == 0)
                {
                    Console.WriteLine("  В базе данных нет книг.");
                    return;
                }

                Console.WriteLine($"{"ID",-5} {"Название",-30} {"Автор",-25} {"Год",-6} {"Кол-во"}");
                Console.WriteLine(new string('─', 76));

                foreach (var book in books)
                {
                    string st = book.Title.Length > 28 ? book.Title.Substring(0, 28) + ".." : book.Title;
                    string sa = book.Author.Length > 23 ? book.Author.Substring(0, 23) + ".." : book.Author;
                    Console.WriteLine($"{book.Id,-5} {st,-30} {sa,-25} {book.PublishYear,-6} {book.Quantity}");
                }

                Console.WriteLine(new string('─', 76));
                Console.WriteLine($"\n  Всего книг: {books.Count}");
            }
            catch (Exception ex)
            {
                PrintError($"Ошибка: {ex.Message}");
            }
        }


        static void AddBook()
        {
            Console.Clear();
            Console.WriteLine("═══════════════ ДОБАВИТЬ КНИГУ ═══════════════\n");

            try
            {
                string title = ReadNonEmpty("Название книги : ");
                string author = ReadNonEmpty("Автор          : ");
                int year = ReadInt("Год издания    : ", 1, 2100);
                int quantity = ReadInt("Количество     : ", 0, 10000);

                books.Add(new Book
                {
                    Id = nextId++,
                    Title = title,
                    Author = author,
                    PublishYear = year,
                    Quantity = quantity
                });

                PrintSuccess("Книга успешно добавлена!");
            }
            catch (Exception ex)
            {
                PrintError($"Ошибка: {ex.Message}");
            }
        }


        static void SearchBook()
        {
            Console.Clear();
            Console.WriteLine("═══════════════ ПОИСК КНИГИ ═══════════════\n");

            Console.Write("Введите название (или часть): ");
            string keyword = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                PrintError("Запрос не может быть пустым.");
                return;
            }

            try
            {
                var result = books
                    .Where(b => b.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                Console.WriteLine();

                if (result.Count == 0)
                {
                    Console.WriteLine($"  Книги с названием «{keyword}» не найдены.");
                    return;
                }

                Console.WriteLine($"{"ID",-5} {"Название",-30} {"Автор",-25} {"Год",-6} {"Кол-во"}");
                Console.WriteLine(new string('─', 76));

                foreach (var book in result)
                {
                    string st = book.Title.Length > 28 ? book.Title.Substring(0, 28) + ".." : book.Title;
                    string sa = book.Author.Length > 23 ? book.Author.Substring(0, 23) + ".." : book.Author;
                    Console.WriteLine($"{book.Id,-5} {st,-30} {sa,-25} {book.PublishYear,-6} {book.Quantity}");
                }

                Console.WriteLine(new string('─', 76));
                Console.WriteLine($"\n  Найдено: {result.Count}");
            }
            catch (Exception ex)
            {
                PrintError($"Ошибка: {ex.Message}");
            }
        }


        static void DeleteBook()
        {
            Console.Clear();
            Console.WriteLine("═══════════════ УДАЛИТЬ КНИГУ ═══════════════\n");

            ShowAllBooks();
            Console.WriteLine();

            try
            {
                int id = ReadInt("Введите ID книги для удаления: ", 1, int.MaxValue);

                Book book = books.FirstOrDefault(b => b.Id == id);

                if (book == null)
                {
                    PrintError($"Книга с ID = {id} не найдена.");
                    return;
                }

                Console.WriteLine($"\n  Будет удалена: «{book.Title}» — {book.Author}");
                Console.Write("  Подтвердите удаление (y/n): ");
                string confirm = Console.ReadLine()?.Trim().ToLower();

                if (confirm != "y")
                {
                    Console.WriteLine("  Удаление отменено.");
                    return;
                }

                books.Remove(book);
                PrintSuccess($"Книга «{book.Title}» успешно удалена.");
            }
            catch (Exception ex)
            {
                PrintError($"Ошибка: {ex.Message}");
            }
        }

        static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string v = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(v)) return v;
                PrintError("Поле не может быть пустым.");
            }
        }

        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine()?.Trim(), out int v) && v >= min && v <= max)
                    return v;
                PrintError($"Введите число от {min} до {max}.");
            }
        }

        static void PrintError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n  ✖ {msg}");
            Console.ResetColor();
        }

        static void PrintSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✔ {msg}");
            Console.ResetColor();
        }
    }
}
