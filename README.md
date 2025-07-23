# 🌍 Trade Management System

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.1.0-61DAFB)](https://reactjs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-4.9.5-3178C6)](https://www.typescriptlang.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-336791)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

一个现代化的贸易文档管理和合规监控系统，支持全栈开发的企业级应用。

## 📋 项目概述

Trade Management System 是一个为贸易公司设计的综合性管理平台，提供：

- 📄 **贸易文档管理** - 进出口许可证、原产地证书等文档的全生命周期管理
- 🚨 **合规警报系统** - 实时监控贸易合规性，自动生成风险预警
- 👤 **用户认证授权** - 基于 JWT 的安全认证和 ASP.NET Identity 用户管理
- 📊 **实时数据同步** - 前后端实时数据交互，支持多用户协作

## 🏗️ 技术架构

### 后端技术栈
- **框架**: .NET 8.0 Web API
- **数据库**: PostgreSQL with Entity Framework Core
- **认证**: JWT Token + ASP.NET Identity
- **API文档**: Swagger/OpenAPI 3.0
- **依赖注入**: Built-in DI Container

### 前端技术栈
- **框架**: React 19.1.0 with TypeScript
- **构建工具**: Create React App
- **状态管理**: React Hooks
- **HTTP客户端**: Fetch API
- **样式**: CSS3 with Flexbox/Grid

### 数据库设计
```
TradeDocuments
├── 文档基本信息 (类型、国家、状态、公司)
├── 业务数据 (价值、风险等级、有效期)
└── 审计字段 (创建时间、修改时间、操作人)

ComplianceAlerts  
├── 警报信息 (类型、消息、严重级别)
├── 关联关系 (关联文档、创建时间)
└── 状态管理 (是否已读、来源)

AspNetIdentity Tables
├── 用户管理 (用户信息、角色、权限)
└── 认证数据 (登录日志、令牌管理)
```

## 🚀 快速开始

### 环境要求
- .NET 8.0 SDK
- Node.js 18+ & npm
- PostgreSQL 15+
- Git

### 1. 克隆项目
```bash
git clone https://github.com/your-username/trade-demo.git
cd trade-demo
```

### 2. 数据库配置
```bash
# 创建 PostgreSQL 数据库
createdb trademanagement

# 更新连接字符串 (backend/TradeManagementApi/appsettings.json)
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=trademanagement;Username=your_user;Password=your_password"
  }
}
```

### 3. 启动后端 API
```bash
cd backend/TradeManagementApi
dotnet restore
dotnet run --urls="http://0.0.0.0:5002"
```

### 4. 启动前端应用
```bash
cd frontend/trade-management
npm install
npm start
```

### 5. 访问应用
- **前端应用**: http://localhost:3000
- **API 接口**: http://localhost:5002
- **Swagger 文档**: http://localhost:5002/swagger
- **默认管理员**: admin@trademanagement.com / Admin123!

## 📁 项目结构

```
trade-demo/
├── backend/
│   └── TradeManagementApi/
│       ├── Controllers/          # API 控制器
│       │   ├── TradeDocumentsController.cs
│       │   ├── ComplianceAlertsController.cs
│       │   └── AuthController.cs
│       ├── Models/               # 数据模型
│       │   ├── TradeModels.cs
│       │   └── UserModels.cs
│       ├── Data/                 # 数据访问层
│       │   └── TradeDbContext.cs
│       ├── Services/             # 业务服务
│       │   └── JwtService.cs
│       ├── Migrations/           # 数据库迁移
│       └── Program.cs            # 应用入口
├── frontend/
│   └── trade-management/
│       ├── src/
│       │   ├── App.tsx           # 主应用组件
│       │   ├── App.css           # 样式文件
│       │   └── index.tsx         # 应用入口
│       ├── public/               # 静态资源
│       └── package.json          # 依赖配置
├── docs/                         # 项目文档
├── scripts/                      # 部署脚本
└── README.md                     # 项目说明
```

## 🔧 核心功能

### 贸易文档管理
- ✅ 文档 CRUD 操作 (增删改查)
- ✅ 文档状态流转 (草稿→待审批→已批准/拒绝)
- ✅ 风险等级评估 (低/中/高风险自动分类)
- ✅ 按国家、类型、状态筛选

### 合规警报系统
- ✅ 实时合规性监控
- ✅ 自动风险预警生成
- ✅ 警报严重级别分类 (1-5级)
- ✅ 系统级法规更新通知

### 用户认证与授权
- ✅ JWT Token 无状态认证
- ✅ 用户注册和登录
- ✅ 密码强度验证
- ✅ Token 自动续期机制

### API 特性
- ✅ RESTful API 设计
- ✅ Swagger UI 自动文档
- ✅ CORS 跨域支持
- ✅ 异步数据库操作
- ✅ 统一错误处理

## 🧪 测试

### 后端测试
```bash
cd backend/TradeManagementApi
dotnet test
```

### 前端测试
```bash
cd frontend/trade-management
npm test
```

### 手动测试
使用提供的 `TradeManagementApi.http` 文件进行 API 测试，或访问 Swagger UI 进行交互式测试。

## 📦 部署

### Docker 部署 (推荐)
```bash
# 构建并启动所有服务
docker-compose up -d

# 查看运行状态
docker-compose ps
```

### 传统部署
```bash
# 后端发布
cd backend/TradeManagementApi
dotnet publish -c Release -o ./publish

# 前端构建
cd frontend/trade-management
npm run build
```

## 🔒 安全性

- **JWT 认证**: 32字符以上安全密钥，HMAC-SHA256 签名
- **密码策略**: 强制大小写字母、数字，最少6位
- **HTTPS 支持**: 生产环境强制 SSL/TLS
- **SQL 注入防护**: Entity Framework 参数化查询
- **CORS 限制**: 精确控制跨域访问源

## 🤝 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📝 待办事项

- [ ] 添加单元测试覆盖
- [ ] 实现文档附件上传
- [ ] 添加数据导出功能
- [ ] 集成第三方合规 API
- [ ] 多语言国际化支持
- [ ] 移动端响应式优化

## 📄 License

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 👥 团队

- **主要开发者**: [@your-username](https://github.com/your-username)
- **架构设计**: .NET 8.0 + React + PostgreSQL
- **开发环境**: WSL2 + VS Code + Podman

## 🙏 致谢

感谢以下开源项目的支持：
- [ASP.NET Core](https://github.com/dotnet/aspnetcore)
- [Entity Framework Core](https://github.com/dotnet/efcore)
- [React](https://github.com/facebook/react)
- [PostgreSQL](https://www.postgresql.org/)

---

**📞 联系我们**

如有问题或建议，请提交 [Issue](https://github.com/your-username/trade-demo/issues) 或发送邮件至 your-email@example.com

⭐ 如果这个项目对您有帮助，请给我们一个 Star！