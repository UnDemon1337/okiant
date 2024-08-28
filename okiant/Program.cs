using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static Dictionary<string, string> users = new Dictionary<string, string>(); // Хранение пользователей
    static Dictionary<string, int> ratings = new Dictionary<string, int>(); // Хранение рейтингов
    static string player1Login = null;
    static string player2Login = null;

    static void Main()
    {
        while (true)
        {
            ShowMainMenu();
        }
    }

    static void ShowMainMenu()
    {
        Console.WriteLine("Меню:");
        Console.WriteLine("1. Вход для первого игрока");
        Console.WriteLine("2. Вход для второго игрока");
        Console.WriteLine("3. Зарегистрировать нового пользователя");
        Console.WriteLine("4. Отобразить всех пользователей");
        Console.WriteLine("5. Выйти");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();
        if (choice == "1")
        {
            if (player1Login == null && LoginUser(out player1Login))
            {
                Console.WriteLine($"Первый игрок вошел как: {player1Login}");
            }

        }
        else if (choice == "2")
        {
            if (player2Login == null && LoginUser(out player2Login))
            {
                if (player2Login == player1Login)
                {
                    Console.WriteLine("Нельзя использовать один и тот же аккаунт для обоих игроков.");
                    player2Login = null;
                }
                else
                {
                    Console.WriteLine($"Второй игрок вошел как: {player2Login}");
                }
            }
        }
        else if (choice == "3")
        {
            RegisterUser();
        }
        else if (choice == "4")
        {
            DisplayAllUsers();
        }
        else if (choice == "5")
        {
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine("Некорректный выбор. Попробуйте снова.");
        }

        // Проверка, что оба игрока вошли в систему
        if (player1Login != null && player2Login != null)
        {
            StartGame();
        }
    }

    static void RegisterUser()
    {
        Console.WriteLine("Регистрация нового пользователя");
        Console.Write("Введите логин и пароль через пробел: ");
        var input = Console.ReadLine().Split(' ');

        if (input.Length != 2)
        {
            Console.WriteLine("Некорректный ввод. Попробуйте снова.");
            return;
        }

        var login = input[0];
        var password = input[1];

        if (users.ContainsKey(login))
        {
            Console.WriteLine("Логин уже существует. Попробуйте другой.");
            return;
        }

        var passwordHash = HashPassword(password);
        users[login] = passwordHash;
        ratings[login] = 1000; // Начальный рейтинг

        Console.WriteLine("Пользователь зарегистрирован успешно.");
    }

    static bool LoginUser(out string login)
    {
        Console.WriteLine("Вход");
        Console.Write("Введите логин: ");
        login = Console.ReadLine();
        Console.Write("Введите пароль: ");
        var password = Console.ReadLine();

        if (users.ContainsKey(login))
        {
            var storedHash = users[login];
            if (VerifyPassword(password, storedHash))
            {
                return true;
            }
        }

        Console.WriteLine("Неверный логин или пароль. Попробуйте снова.");
        login = null;
        return false;
    }

    static void DisplayAllUsers()
    {
        if (users.Count > 0)
        {
            Console.WriteLine("Зарегистрированные пользователи:");
            foreach (var user in users)
            {
                string rating = ratings.ContainsKey(user.Key) ? ratings[user.Key].ToString() : "N/A";
                Console.WriteLine($"Логин: {user.Key}, Хэш пароля: {user.Value}, Рейтинг: {rating}");
            }
        }
        else
        {
            Console.WriteLine("Нет пользователей");
        }
    }

    static void StartGame()
    {
        string[][] arrays = {
            new string[] { "листок", "эмблема" },
            new string[] { "листок", "дождь" },
            new string[] { "эмблема", "цветок" },
            new string[] { "звезда", "птицы" },
            new string[] { "цветок", "дождь" },
            new string[] { "звезда", "эмблема" },
            new string[] { "звезда", "солнце" },
            new string[] { "солнце", "листок" },
            new string[] { "солнце", "сакура" },
            new string[] { "листок", "птицы" },
            new string[] { "птицы", "цветок" },
            new string[] { "сакура", "дождь" },
            new string[] { "птицы", "сакура" },
            new string[] { "звезда", "дождь" },
            new string[] { "сакура", "эмблема" },
            new string[] { "солнце", "цветок" }
        };

        // Перемешивание массивов
        Random rnd = new Random();
        arrays = arrays.OrderBy(x => rnd.Next()).ToArray();

        // Создание игрового поля 4x4
        string[][] board = new string[4][];
        for (int i = 0; i < 4; i++)
        {
            board[i] = new string[4];
            for (int j = 0; j < 4; j++)
            {
                board[i][j] = string.Join(", ", arrays[i * 4 + j]);
            }
        }

        // Печать стартового поля
        PrintBoard(board);

        string currentPlayerLogin = player1Login;
        string[] lastSelection = null;
        bool isFirstMove = true;

        while (true)
        {
            // Определение допустимых ходов
            var validMoves = GetValidMoves(board, lastSelection, currentPlayerLogin == player1Login && isFirstMove);

            // Если нет допустимых ходов, конец игры
            if (validMoves.Count == 0)
            {
                Console.WriteLine($"{currentPlayerLogin} не может сделать ход. Игра окончена.");
                break;
            }

            // Выбор хода
            Console.WriteLine($"{currentPlayerLogin}, выберите комбинацию:");
            for (int i = 0; i < validMoves.Count; i++)
            {
                var move = validMoves[i];
                Console.WriteLine($"{i + 1}: [{move.Item1}, {move.Item2}] : {board[move.Item1][move.Item2]}");
            }

            // Ввод номера комбинации
            int selectedIndex;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out selectedIndex) && selectedIndex > 0 && selectedIndex <= validMoves.Count)
                {
                    break;
                }
                Console.WriteLine("Некорректный ввод. Попробуйте снова.");
            }

            var selectedMove = validMoves[selectedIndex - 1];

            // Обновление поля
            board[selectedMove.Item1][selectedMove.Item2] = currentPlayerLogin;
            lastSelection = arrays[selectedMove.Item1 * 4 + selectedMove.Item2];

            // Проверка на победу
            if (CheckWin(board, currentPlayerLogin))
            {
                Console.WriteLine($"{currentPlayerLogin} выиграл!");
                PrintBoard(board);
                UpdateRating(currentPlayerLogin, 10); // Победителю добавляется 10 рейтингов
                UpdateRating(currentPlayerLogin == player1Login ? player2Login : player1Login, -10); // Проигравшему убирается 10 рейтингов
                break;
            }

            // Смена игрока
            currentPlayerLogin = currentPlayerLogin == player1Login ? player2Login : player1Login;

            // Печать игрового поля
            PrintBoard(board);

            // Первый ход завершён, снимаем ограничение на выбор центральных ячеек
            if (isFirstMove) isFirstMove = false;
        }

        // Возвращаемся в меню после завершения игры
        player1Login = null;
        player2Login = null;
        ShowMainMenu();
    }

    static void PrintBoard(string[][] board)
    {
        Console.WriteLine("Текущая доска:");
        Console.WriteLine("╔═══════════════════╦═══════════════════╦═══════════════════╦═══════════════════╗");
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Console.Write($"║ {board[i][j].PadRight(17)} ");
            }
            Console.WriteLine("║");
            if (i < 3)
                Console.WriteLine("╠═══════════════════╬═══════════════════╬═══════════════════╬═══════════════════╣");
        }
        Console.WriteLine("╚═══════════════════╩═══════════════════╩═══════════════════╩═══════════════════╝");
    }

    static System.Collections.Generic.List<(int, int)> GetValidMoves(string[][] board, string[] lastSelection, bool isFirstMove)
    {
        var validMoves = new System.Collections.Generic.List<(int, int)>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (board[i][j] != player1Login && board[i][j] != player2Login) // ячейка не занята
                {
                    if (isFirstMove)
                    {
                        // Первый ход: только центральные ячейки (6, 7, 10, 11)
                        if ((i == 1 || i == 2) && (j == 1 || j == 2))
                        {
                            validMoves.Add((i, j));
                        }
                    }
                    else
                    {
                        // Проверка наличия элемента из предыдущего выбора
                        if (lastSelection != null && lastSelection.Any(element => board[i][j].Contains(element)))
                        {
                            validMoves.Add((i, j));
                        }
                    }
                }
            }
        }
        return validMoves;
    }

    static bool CheckWin(string[][] board, string player)
    {
        // Проверка по горизонтали и вертикали
        for (int i = 0; i < 4; i++)
        {
            if (board[i].All(x => x == player) || board.All(row => row[i] == player))
                return true;
        }

        // Проверка по диагоналям
        if ((board[0][0] == player && board[1][1] == player && board[2][2] == player && board[3][3] == player) ||
            (board[0][3] == player && board[1][2] == player && board[2][1] == player && board[3][0] == player))
            return true;

        // Проверка на квадрат
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i][j] == player && board[i + 1][j] == player &&
                    board[i][j + 1] == player && board[i + 1][j + 1] == player)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static void UpdateRating(string player, int delta)
    {
        if (ratings.ContainsKey(player))
        {
            ratings[player] += delta;
        }
    }

    static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    static bool VerifyPassword(string password, string hashedPassword)
    {
        var hash = HashPassword(password);
        return hash == hashedPassword;
    }
}
