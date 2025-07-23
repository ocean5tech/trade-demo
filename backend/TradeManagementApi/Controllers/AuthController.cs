using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TradeManagementApi.Models;
using TradeManagementApi.Services;

namespace TradeManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<JwtResponse>> Register(RegisterRequest request)
    {
        try
        {
            // 🔧 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("👤 注册请求: {Email}", request.Email);

            // 检查用户是否已存在
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("❌ 用户已存在: {Email}", request.Email);
                return BadRequest(new { message = "该邮箱地址已被注册" });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                Company = request.Company,
                EmailConfirmed = false // 可以后续添加邮箱确认功能
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("❌ 注册失败: {Email}, 错误: {Errors}", 
                    request.Email, string.Join(", ", errorMessages));
                
                return BadRequest(new { 
                    message = "注册失败",
                    errors = errorMessages
                });
            }

            // 为新用户分配默认角色
            await _userManager.AddToRoleAsync(user, "Viewer");

            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("✅ 用户注册成功: {Email}", user.Email);

            return Ok(new JwtResponse
            {
                Token = token,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Company = user.Company,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册过程中发生异常: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "注册过程中发生错误，请稍后重试" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<JwtResponse>> Login(LoginRequest request)
    {
        try
        {
            // 🔧 输入验证
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("🔐 登录请求: {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("❌ 用户未找到: {Email}", request.Email);
                return Unauthorized(new { message = "邮箱或密码错误" });
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("❌ 用户已被禁用: {Email}", request.Email);
                return Unauthorized(new { message = "账户已被禁用，请联系管理员" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("❌ 账户被锁定: {Email}", request.Email);
                return Unauthorized(new { message = "账户已被锁定，请稍后再试" });
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("❌ 密码错误: {Email}", request.Email);
                return Unauthorized(new { message = "邮箱或密码错误" });
            }

            var token = _jwtService.GenerateToken(user);
            _logger.LogInformation("✅ 登录成功: {Email}", user.Email);

            return Ok(new JwtResponse
            {
                Token = token,
                Email = user.Email ?? string.Empty, // 🔧 处理可能的null值
                FullName = user.FullName,
                Company = user.Company,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录过程中发生异常: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "登录过程中发生错误，请稍后重试" });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            _logger.LogInformation("✅ 用户登出: {Email}", userEmail ?? "Unknown");
            
            return Ok(new { message = "成功登出" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登出过程中发生异常");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "登出过程中发生错误" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<JwtResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { message = "Token不能为空" });
            }

            // 验证旧token（即使已过期）
            var userInfo = _jwtService.GetUserInfoFromToken(request.Token);
            if (userInfo == null)
            {
                return Unauthorized(new { message = "无效的Token" });
            }

            // 获取用户信息
            var user = await _userManager.FindByIdAsync(userInfo.UserId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "用户不存在或已被禁用" });
            }

            // 生成新token
            var newToken = _jwtService.GenerateToken(user);
            _logger.LogInformation("✅ Token刷新成功: {Email}", user.Email);

            return Ok(new JwtResponse
            {
                Token = newToken,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Company = user.Company,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token刷新过程中发生异常");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Token刷新失败，请重新登录" });
        }
    }

    [HttpGet("user-info")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "无效的用户Token" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "用户不存在" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Company = user.Company,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                Roles = roles.ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户信息时发生异常");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "获取用户信息失败" });
        }
    }
}

// 🔧 新增的请求/响应模型
public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<string> Roles { get; set; } = new();
}