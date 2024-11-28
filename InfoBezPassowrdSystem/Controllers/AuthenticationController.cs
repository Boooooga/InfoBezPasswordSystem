using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InfoBezPasswordSystem;
using InfoBezPasswordSystem.DTO;
using System.Security.Cryptography;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly JWTService _jwtService;
    private readonly AuthService _authService;

    public AuthenticationController(AuthService authService, AppDbContext dbContext, JWTService jwtService, IConfiguration configuration)
    {
        _authService = authService;
        _dbContext = dbContext;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    [HttpPost("sign-up")]
    public IActionResult SignUp([FromForm] string username, [FromForm] string password)
    {
        // Проверка на наличие пользователя с таким логином
        if (_dbContext.Users.Any(u => u.Username == username))
        {
            return BadRequest($"Пользователь с именем {username} уже зарегистрирован");
        }
        // Проверка на длину пароля (чтобы было ещё что-то для ошибки 400)
        if (password.Length < 6)
        {
            return BadRequest("Пароль должен состоять хотя бы из 6 символов.");
        }

        // Регистрация нового пользователя
        _authService.Register(username, password, _dbContext);
        return StatusCode(201, new { message = $"Пользователь {username} успешно зарегистрирован." });
    }
    [HttpPost("auth")]
    public IActionResult Login([FromForm] string username, [FromForm] string password)
    {
        if (!_authService.Authenticate(username, password, _dbContext))
        {
            return StatusCode(403, new { message = "Неправильное имя пользователя или пароль." });
        }

        // Генерация JWT токенов
        var accessToken = _jwtService.GenerateToken(username, 15);  // Access Token
        var refreshToken = _jwtService.GenerateToken(username, 1440); // Refresh Token

        // Установка токенов в cookies
        Response.Cookies.Append("AccessToken", accessToken, new CookieOptions { SameSite = SameSiteMode.Strict });
        Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        {
            Path = "/Authentication/refresh",
            SameSite = SameSiteMode.Strict
        });

        return Ok(new { message = "Успешный вход." });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        if (refreshToken == null)
            return Unauthorized();

        var principal = _jwtService.GetPrincipalFromExpiredToken(refreshToken);
        if (principal == null)
            return Unauthorized();

        var username = principal.Identity?.Name;
        if (username == null)
            return Unauthorized();


        var newAccessToken = _jwtService.GenerateToken(username, 15); // Новый Access token
        var newRefreshToken = _jwtService.GenerateToken(username, 1440); // Новый Refresh token

        Response.Cookies.Append("AccessToken", newAccessToken);
        Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
        {
            Path = "/Authentication/refresh", // Ограничиваем путь только для /refresh
            SameSite = SameSiteMode.Strict
        });

        return Ok(new { message = "Токены обновлены." });
    }

}
