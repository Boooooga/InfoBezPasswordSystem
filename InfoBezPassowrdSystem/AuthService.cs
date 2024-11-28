using System.Security.Cryptography;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class AuthService
{
    // Метод для генерации соли
    private byte[] GenerateSalt()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] salt = new byte[16]; // 16 байтов для соли (можно изменить размер)
            rng.GetBytes(salt);
            return salt;
        }
    }

    // Метод для хеширования пароля с солью
    private byte[] ComputeHash(string password, byte[] salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = salt.Concat(System.Text.Encoding.UTF8.GetBytes(password)).ToArray();
            return sha256.ComputeHash(saltedPassword);
        }
    }

    // Метод для регистрации пользователя
    public void Register(string username, string password, AppDbContext dbContext)
    {
        // Генерация соли
        var salt = GenerateSalt();

        // Хеширование пароля с солью
        var passwordHash = ComputeHash(password, salt);

        // Создание нового пользователя
        var user = new User
        {
            Username = username,
            Salt = salt,
            PasswordHash = passwordHash
        };

        // Добавление пользователя в базу данных
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }

    // Метод для аутентификации пользователя
    public bool Authenticate(string username, string password, AppDbContext dbContext)
    {
        // Поиск пользователя по логину
        var user = dbContext.Users.SingleOrDefault(u => u.Username == username);
        if (user == null)
        {
            return false;  // Логин не найден
        }

        // Вычисляем хеш от введенного пароля с солью из базы
        var passwordHash = ComputeHash(password, user.Salt);

        // Сравниваем хеши
        return passwordHash.SequenceEqual(user.PasswordHash);
    }
}