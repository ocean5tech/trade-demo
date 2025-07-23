using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TradeManagementApi.Models;

// 扩展用户模型
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Company { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
}

// 登录请求模型
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

// 注册请求模型
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    public string Company { get; set; } = string.Empty;
}

// JWT响应模型
public class JwtResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}