using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Ethereal_api.Auth;

public class TokenService
{
    public class TokenResult
    {
        public string? Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
    }

    public interface ITokenService
    {
        // 生成权限token
        TokenResult GenerateAccessToken(IEnumerable<Claim> claims);

        // 获取过期token的密钥
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

        // 生成刷新token
        string GenerateRefreshToken();
    }

    public class RefreshRequest
    {
        // 返回有权限token
        [Required] public string AccessToken { get; set; } = string.Empty;

        // 返回能够刷新旧的token的刷新token
        [Required] public string RefreshToken { get; set; } = string.Empty;
    }
}