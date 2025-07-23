using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TradeManagementApi.Models;

namespace TradeManagementApi.Services;

public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        
        // 🔧 添加null检查和默认值
        var secretKey = jwtSettings["SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey is not configured in appsettings.json");
        var issuer = jwtSettings["Issuer"] 
            ?? throw new InvalidOperationException("JWT Issuer is not configured in appsettings.json");
        var audience = jwtSettings["Audience"] 
            ?? throw new InvalidOperationException("JWT Audience is not configured in appsettings.json");
        
        // 🔧 安全的int解析
        var expiryMinutesString = jwtSettings["ExpiryMinutes"] ?? "60";
        if (!int.TryParse(expiryMinutesString, out var expiryMinutes))
        {
            expiryMinutes = 60; // 默认60分钟
        }

        // 🔧 验证密钥长度
        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long for security");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 🔧 安全的Claims创建，处理可能的null值
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("Company", user.Company ?? string.Empty),
            new Claim("IsActive", user.IsActive.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64),
            new Claim(JwtRegisteredClaimNames.Nbf, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, 
                new DateTimeOffset(DateTime.UtcNow.AddMinutes(expiryMinutes)).ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        // 🔧 检查secretKey是否存在
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"] ?? "TradeManagementAPI",
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"] ?? "TradeManagementClient",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (SecurityTokenException)
        {
            // Token验证失败
            return null;
        }
        catch (Exception)
        {
            // 其他异常
            return null;
        }
    }

    /// <summary>
    /// 从Token中提取用户信息
    /// </summary>
    /// <param name="token">JWT Token</param>
    /// <returns>用户信息，如果Token无效则返回null</returns>
    public TokenUserInfo? GetUserInfoFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
        {
            return null;
        }

        return new TokenUserInfo
        {
            UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            FullName = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            Company = principal.FindFirst("Company")?.Value ?? string.Empty,
            IsActive = bool.Parse(principal.FindFirst("IsActive")?.Value ?? "false")
        };
    }

    /// <summary>
    /// 检查Token是否即将过期（15分钟内）
    /// </summary>
    /// <param name="token">JWT Token</param>
    /// <returns>是否即将过期</returns>
    public bool IsTokenNearExpiry(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(token);
            
            var exp = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            if (exp == null || !long.TryParse(exp, out var expUnix))
            {
                return true; // 如果无法读取过期时间，认为已过期
            }

            var expiryTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            return expiryTime <= DateTime.UtcNow.AddMinutes(15); // 15分钟内过期
        }
        catch
        {
            return true; // 异常时认为已过期
        }
    }
}

/// <summary>
/// 从Token中提取的用户信息
/// </summary>
public class TokenUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}