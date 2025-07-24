using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TradeManagementApi.Data;
using TradeManagementApi.Models;
using TradeManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 添加JWT认证到Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 添加数据库上下文
builder.Services.AddDbContext<TradeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加Identity服务
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // 密码策略
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // 用户策略
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<TradeDbContext>()
.AddDefaultTokenProviders();

// 添加JWT认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});

// 注册JWT服务
builder.Services.AddScoped<JwtService>();

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", 
                "http://localhost:3001",
                "http://18.183.240.121:3000",  // 生产环境前端
                "https://18.183.240.121:3000"  // HTTPS 支持
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 自动应用数据库迁移和种子数据
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TradeDbContext>();
        context.Database.EnsureCreated();
        
        // 创建默认管理员用户
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "admin@trademanagement.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "系统管理员",
                Company = "Trade Management Inc."
            };
            
            await userManager.CreateAsync(adminUser, "Admin123!");
            Console.WriteLine($"✅ 创建默认管理员用户: {adminEmail}");
        }
        
        Console.WriteLine("✅ 数据库连接成功");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ 数据库连接失败: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseAuthentication(); // 添加认证中间件
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Trade Management API启动中...");
Console.WriteLine("📡 API地址: http://0.0.0.0:5000");
Console.WriteLine("🗄️ 数据库: PostgreSQL with Entity Framework");
Console.WriteLine("🔐 认证: JWT Token + ASP.NET Identity");
Console.WriteLine("📚 Swagger文档: http://localhost:5000/swagger");
Console.WriteLine("👤 默认管理员: admin@trademanagement.com / Admin123!");

app.Run();