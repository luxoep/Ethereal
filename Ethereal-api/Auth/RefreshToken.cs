using System.Security.Cryptography;
using System.Text;
using Ethereal_api.IService;
using Microsoft.EntityFrameworkCore;

namespace Ethereal_api.Auth;

public class RefreshToken
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public RefreshToken(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    private static string HashToken(string token)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
            byte[] hashBytes = sha256Hash.ComputeHash(tokenBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// 将刷新token转换为SHA256存入数据库
    /// </summary>
    /// <param name="userid">用户id</param>
    /// <param name="email">用户邮箱</param>
    /// <param name="refreshToken">生成的refreshToken</param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task SaveAsync(int userid, string email, string refreshToken)
    {
        if (!int.TryParse(_configuration["Jwt:RefreshTokenExpiresDays"], out int days))
        {
            days = 7;
        }

        Entities.EtherealUser? etherealUser = await _dbContext.ethereal_user
            .FirstOrDefaultAsync(u => u.UserId == userid && u.Email == email);

        if (etherealUser == null)
            throw new UnauthorizedAccessException("User does not exist or information does not match.");

        etherealUser.RefreshToken = HashToken(refreshToken);
        etherealUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(days);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 验证刷新token是否在有效时间内
    /// </summary>
    /// <param name="refreshToken">生成的refreshToken</param>
    /// <param name="userId">用户id</param>
    /// <returns></returns>
    public async Task<Entities.EtherealUser?> ValidateAsync(string refreshToken, int userId)
    {
        // 将refreshToken转换为SHA256
        string hash = HashToken(refreshToken);

        // 在判断数据库中RefreshTokenExpiry是否过期
        return await _dbContext.ethereal_user
            .Where(u => u.UserId == userId &&
                        u.RefreshToken == hash &&
                        u.RefreshTokenExpiry > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 用户手动登出或管理员手动登出
    /// </summary>
    /// <param name="userId">用户id</param>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task RevokeAsync(int userId)
    {
        Entities.EtherealUser? etherealUser = await _dbContext.ethereal_user
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (etherealUser == null)
            throw new UnauthorizedAccessException("User does not exist or information does not match.");

        etherealUser.RefreshToken = null;
        etherealUser.RefreshTokenExpiry = null;

        await _dbContext.SaveChangesAsync();
    }
}