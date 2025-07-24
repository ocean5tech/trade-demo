# 🛠️ 本地开发指南

## 📋 开发流程

### 标准工作流程
```
1. 本地修改代码
2. 本地启动服务测试
3. 功能验证完成
4. 提交到 GitHub
5. 自动部署到生产环境
```

---

## 🚀 本地开发环境启动

### 方式1: 使用 Podman (推荐)

```bash
# 进入项目目录
cd /home/wyatt/dev-projects/trade-demo

# 启动完整开发环境
./scripts/podman-build.sh

# 或者手动启动
podman-compose -f docker-compose.podman.yml up -d
```

### 方式2: 分别启动前后端

#### 启动后端 API
```bash
cd backend/TradeManagementApi

# 安装依赖并启动
dotnet restore
dotnet run

# 后端将在 http://localhost:5000 启动
# Swagger 文档: http://localhost:5000/swagger
```

#### 启动前端
```bash
cd frontend/trade-management

# 安装依赖
npm install

# 启动开发服务器
npm start

# 前端将在 http://localhost:3000 启动
```

#### 启动数据库 (如需要)
```bash
# 使用 Podman 启动 PostgreSQL
podman run -d --name dev-postgres \
  -e POSTGRES_DB=trademanagement \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres123 \
  -p 5432:5432 \
  postgres:15-alpine
```

---

## 🧪 本地测试检查清单

### ✅ 功能测试
- [ ] 前端页面正常加载
- [ ] API 连接状态显示在线
- [ ] 仪表板数据正常显示
- [ ] 文档管理 CRUD 功能测试：
  - [ ] ➕ 创建新文档
  - [ ] ✏️ 编辑现有文档  
  - [ ] 🗑️ 删除文档
- [ ] 表单验证工作正常
- [ ] 错误处理正确显示

### ✅ 界面测试
- [ ] 响应式设计 (调整浏览器窗口大小)
- [ ] 模态框正常打开/关闭
- [ ] 按钮交互效果正常
- [ ] 加载状态显示正确

### ✅ API 测试
```bash
# 测试 API 端点
curl http://localhost:5000/api/tradedocuments
curl http://localhost:5000/api/compliancealerts

# 或访问 Swagger UI
open http://localhost:5000/swagger
```

---

## 🔄 开发工作流程

### 1. 开始开发
```bash
# 拉取最新代码
git pull origin main

# 创建功能分支 (可选)
git checkout -b feature/new-feature

# 启动本地环境
./scripts/podman-build.sh
```

### 2. 进行修改
```bash
# 修改代码
# 实时查看变化: http://localhost:3000
```

### 3. 本地测试
```bash
# 运行测试
cd frontend/trade-management && npm test
cd backend/TradeManagementApi && dotnet test

# 手动功能测试 (按照上面的检查清单)
```

### 4. 提交代码
```bash
# 检查修改
git status
git diff

# 添加修改
git add .

# 提交 (使用描述性消息)
git commit -m "✨ Add: 具体功能描述

- 详细说明修改内容
- 测试步骤和结果
- 任何需要注意的事项"

# 推送到 GitHub (触发自动部署)
git push origin main
```

### 5. 监控部署
```bash
# 查看 GitHub Actions
open https://github.com/ocean5tech/trade-demo/actions

# 部署完成后验证生产环境
open http://18.183.240.121:3000
```

---

## 🛠️ 开发工具配置

### VS Code 推荐扩展
```
- C# Dev Kit (后端开发)
- ES7+ React/Redux/React-Native snippets (前端开发)
- Prettier (代码格式化)
- GitLens (Git 增强)
- Docker (容器管理)
```

### 环境变量设置
```bash
# 前端开发环境变量 (.env.local)
REACT_APP_API_URL=http://localhost:5000/api

# 后端开发环境变量
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="Host=localhost;Database=trademanagement;Username=postgres;Password=postgres123"
```

---

## 🚨 故障排除

### 常见问题

#### 端口占用
```bash
# 查看端口使用情况
netstat -tulpn | grep :3000
netstat -tulpn | grep :5000

# 停止占用进程
kill -9 <PID>
```

#### 容器问题
```bash
# 查看容器状态
podman ps -a

# 查看日志
podman logs container_name

# 重启容器
podman restart container_name

# 清理并重新构建
podman system prune -f
./scripts/podman-build.sh
```

#### 依赖问题
```bash
# 前端依赖
cd frontend/trade-management
rm -rf node_modules package-lock.json
npm install

# 后端依赖
cd backend/TradeManagementApi
dotnet clean
dotnet restore
```

---

## 📝 开发最佳实践

### Git 提交规范
```
✨ Add: 新功能
🔧 Fix: 错误修复  
📚 Docs: 文档更新
🎨 Style: 代码格式化
♻️ Refactor: 代码重构
🧪 Test: 测试相关
🚀 Deploy: 部署相关
```

### 代码审查检查点
- [ ] 代码格式化正确
- [ ] 没有 console.log 调试信息
- [ ] 错误处理完善
- [ ] 注释清晰
- [ ] 性能影响评估

### 测试覆盖
- [ ] 单元测试 (如有)
- [ ] 集成测试
- [ ] 用户界面测试
- [ ] 边界条件测试

---

**💡 记住**: 本地测试通过后再推送，确保生产环境稳定！