# 🔒 Security Configuration Guide

## 配置文件安全

### 1. 数据库连接字符串
创建你的 `appsettings.Local.json` 文件（已在 .gitignore 中排除）：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TradeManagement;Username=实际用户名;Password=实际密码"
  }
}
```

### 2. JWT 密钥配置
在生产环境中，请确保：

- `SecretKey` 至少 32 个字符
- 使用强随机密钥，避免使用示例密钥
- 定期轮换密钥

### 3. 环境变量 (推荐)
生产环境建议使用环境变量：

```bash
export ConnectionStrings__DefaultConnection="Host=prod-server;Database=TradeManagement;Username=prod_user;Password=secure_password"
export JwtSettings__SecretKey="your-super-secure-random-key-here"
```

### 4. 安全检查清单

- [ ] 生产环境连接字符串已配置
- [ ] JWT 密钥已更换为强密钥
- [ ] 默认管理员密码已修改
- [ ] HTTPS 已启用
- [ ] CORS 已限制到特定域名
- [ ] 敏感文件已排除在版本控制外

## 报告安全问题

如发现安全漏洞，请通过以下方式报告：
- 邮箱: security@yourdomain.com
- 请不要在公开 Issue 中报告安全问题