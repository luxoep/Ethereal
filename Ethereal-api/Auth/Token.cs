using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ethereal_api.Auth;

public class Token : TokenService.ITokenService
{
    private readonly IConfiguration _configuration;

    public Token(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// 生成短期有效地访问令牌（JWT）
    /// </summary>
    /// <param name="claims"> JWT 里的用户信息</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TokenService.TokenResult GenerateAccessToken(IEnumerable<Claim> claims)
    {
        // 1. 读取app settings中的密钥（SecretKey）和Token有效时间（ExpiresMinutes）,不能为空
        string secretKey = _configuration["Jwt:SecretKey"]!;

        string expiresMinutes = _configuration["Jwt:ExpiresMinutes"] ??
                                throw new InvalidOperationException("Jwt:ExpiresMinutes not set");

        // 2. 生成签名密钥，HMAC-SHA256对称加密，生成安全签名密钥，用来保护Token不会被篡改，并且将密钥（SecretKey）转换为加密用的密钥对象
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // 2.1. secretKey建议长度256位（32字节），否则会提示Error不够安全，将密钥（SecretKey）使用HMAC-SHA256进行签名
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. 创建JWT对象
        // issuer: 令牌发行者,用于确认Token的来源
        // audience: 用于标识该令牌的预期接收者，通常是客户端应用程序、API 资源或服务 
        // claims: 存储用户的身份信息
        // expires: 有效时间
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(expiresMinutes));

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            // signingCredentials: 用于签名token，关联[2.1]
            signingCredentials: credentials
        );

        return new TokenService.TokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expires = expiresAt
        };
    }

    /// <summary>
    /// 从过期的访问令牌中提取用户信息（用于刷新 Token）
    /// </summary>
    /// <param name="token">旧的Token</param>
    /// <returns></returns>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // 1. 当短期token过期，并验证refresh token是否有效并进行匹配，需要忽略过期检查，再解析并取出claims
        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
        {
            // 验证令牌发行者
            ValidateIssuer = true,
            // 验证令牌接收者
            ValidateAudience = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            // 检查签名
            ValidateIssuerSigningKey = true,
            // IssuerSigningKey(签名秘钥)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!)),
            // 是否验证Token有效期
            ValidateLifetime = false,
        };

        // 2. 解析和操作 JWT,并安全地读取、验证和操作 JWT
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        ClaimsPrincipal principal = tokenHandler.ValidateToken(
            token,
            tokenValidationParameters,
            out SecurityToken securityToken);

        // 3. 检查解析到的securityToken是否为jwt，并且验证token是否为HS256签名
        // StringComparison 枚举类型，字符串比较行为
        // StringComparison.Ordinal：按字节序比较（区分大小写）、Unicode 值逐字符比较、最快、最稳定
        // StringComparison.OrdinalIgnoreCase：按字节序比较（不区分大小写）、不考虑文化语言、推荐用于技术比较
        // StringComparison.InvariantCulture / InvariantCultureIgnoreCase：基于固定文化规则比较、不依赖系统语言、接近英语规则
        // StringComparison.CurrentCulture / CurrentCultureIgnoreCase：基于当前区域文化比较、根据当前系统语言比较
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    /// <summary>
    /// 生成长期有效地刷新令牌（随机数方式）
    /// </summary>
    /// <returns></returns>
    public string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        // RandomNumberGenerator 基于加密算法，生成随机字节数组
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }
}