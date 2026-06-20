using System.Security.Claims;
using Ethereal_api.Auth;
using Ethereal_api.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ethereal_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService.ITokenService _tokenService;
    private readonly RefreshToken _refreshToken;
    private readonly AppDbContext _dbContext;

    public AuthController(TokenService.ITokenService tokenService, RefreshToken refreshToken, AppDbContext dbContext)
    {
        _tokenService = tokenService;
        _refreshToken = refreshToken;
        _dbContext = dbContext;
    }

    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? login)
    {
        // 判断login信息是否为空
        if (login == null || string.IsNullOrEmpty(login.Email))
            return BadRequest("UserName or PassWord Incorrect");

        // 根据邮箱查找用户，并且IsActive=true
        Entities.EtherealUser? user = await _dbContext.ethereal_user
            .FirstOrDefaultAsync(u => u.Email == login.Email && u.IsActive);

        // 如果user为空，并且验证密码和加密后密文是否匹配，返回提示信息
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            return BadRequest("Invalid username or password");

        // 检查Email是否为空值，等价于if (arg == null) throw new ArgumentNullException(nameof(arg));
        ArgumentNullException.ThrowIfNull(login.Email);

        // 存储用户登录信息
        Claim[] claims = new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role ?? "Member"),
            new Claim("FullName", user.FullName!),
        };

        // 传入用户登录信息，获取登录token
        TokenService.TokenResult accessToken = _tokenService.GenerateAccessToken(claims);

        // 获取刷新token
        string? refreshToken = _tokenService.GenerateRefreshToken();

        // 判断刷新token的字符串null是空字符串还是只包含空格字符
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest("Refresh Token is empty");

        // 通过Userid和Email查询用户，并且将refreshToken存入数据库
        await _refreshToken.SaveAsync(
            user.UserId,
            user.Email,
            refreshToken
        );

        return Ok(new
        {
            access_token = accessToken,
            refresh_token = refreshToken,
        });
    }

    [HttpPost("refreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenService.RefreshRequest refresh)
    {
        if (string.IsNullOrWhiteSpace(refresh.AccessToken)
            || string.IsNullOrWhiteSpace(refresh.RefreshToken))
            return BadRequest("Refresh Token is empty");

        // 从过期的访问令牌中提取用户信息（用于刷新 Token）
        ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(refresh.AccessToken);

        // 获取用户id
        string? userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 获取用户登录邮箱
        string? email = principal.FindFirst(ClaimTypes.Email)?.Value;

        if (userId == null || email == null)
            return BadRequest("UserId or Email Incorrect");

        if (!int.TryParse(userId, out int userIdInt))
            return Unauthorized();

        Entities.EtherealUser? stored = await _refreshToken.ValidateAsync(
            refresh.RefreshToken,
            userIdInt);

        if (stored == null)
            return Unauthorized("Invalid refresh token");

        // 用户手动登出或管理员手动登出
        await _refreshToken.RevokeAsync(userIdInt);

        // 根据用户信息获取一个新的登录token
        TokenService.TokenResult newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);

        // 生成一个全新的刷新token
        string? refreshToken = _tokenService.GenerateRefreshToken();

        // 存储最新的刷新token
        await _refreshToken.SaveAsync(
            userIdInt,
            email,
            refreshToken);

        return Ok(new
        {
            access_token = newAccessToken,
            refresh_token = refreshToken,
        });
    }
}