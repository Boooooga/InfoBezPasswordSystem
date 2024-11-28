public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public byte[] Salt { get; set; }  // Соль
    public byte[] PasswordHash { get; set; }  // Хеш от соли + пароляAdd-Migration FixPasswordHashType

}