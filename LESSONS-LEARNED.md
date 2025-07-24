# 🎓 部署故障排除经验总结

## 📋 概述

本文档总结了 Trade Demo 项目从本地开发到生产部署过程中遇到的问题、解决方案和最佳实践。重点关注配置文件的一致性和环境差异处理。

---

## 🚨 遇到的主要问题

### 1. GitHub 推送保护问题
**问题**: 将 SSH 私钥提交到代码仓库，触发 GitHub 安全扫描
```
remote: - Push cannot contain secrets
remote: —— GitHub SSH Private Key ————————————————————————————
```

**根本原因**: 在文档文件中直接包含了 SSH 私钥内容

**解决方案**:
- 使用 `git reset --hard` 重写历史
- 将敏感信息移到 `.gitignore`
- 使用 GitHub Secrets 管理敏感数据

**经验教训**: 
- ✅ 永远不要在代码中包含密钥、密码等敏感信息
- ✅ 使用环境变量和密钥管理服务
- ✅ 配置 `.gitignore` 排除敏感文件

### 2. 磁盘空间不足问题
**问题**: EC2 免费实例磁盘空间限制
```
❌ Insufficient disk space. Available: 1902MB, Required: 2GB
```

**根本原因**: 
- 免费 EC2 实例磁盘空间有限
- 部署检查设置过于严格
- 没有预先清理系统

**解决方案**:
```yaml
# .github/workflows/ci-cd.yml
REQUIRED_SPACE=1048576  # 降低到 1GB
# 添加自动清理
docker system prune -af 2>/dev/null || true
sudo apt-get clean 2>/dev/null || sudo yum clean all 2>/dev/null || true
rm -rf /tmp/* 2>/dev/null || true
```

**经验教训**:
- ✅ 根据实际硬件资源调整部署要求
- ✅ 在部署前自动清理临时文件
- ✅ 监控磁盘空间使用情况

### 3. API 端点配置不一致问题
**问题**: 前端无法连接到后端 API，显示 404 错误

**根本原因**: 多个配置文件中的端口和地址不一致

**问题配置对比**:

| 文件 | 错误配置 | 正确配置 |
|------|----------|----------|
| `frontend/src/App.tsx` | `localhost:5002` | 动态配置 |
| `backend/Program.cs` | 缺少生产环境 CORS | 包含所有环境 |
| `nginx.conf` | `proxy_pass http://backend:5000/` | `proxy_pass http://backend:5000/api/` |

---

## 🔧 配置文件一致性矩阵

### 端口配置一致性

| 组件 | 配置文件 | 配置项 | 值 | 说明 |
|------|----------|--------|-----|------|
| **前端容器** | `docker-compose.yml` | `ports` | `3000:80` | 外部访问端口 |
| **后端容器** | `docker-compose.yml` | `ports` | `5000:5000` | API 访问端口 |
| **安全组** | AWS Console | Inbound Rules | 22, 3000, 5000 | 网络访问控制 |
| **前端代码** | `App.tsx` | `API_BASE_URL` | 动态配置 | 环境感知 |
| **Nginx** | `nginx.conf` | `proxy_pass` | `backend:5000/api/` | 内部路由 |
| **后端代码** | `Program.cs` | `CORS Origins` | 包含所有环境 | 跨域配置 |

### CORS 配置一致性

```csharp
// backend/Program.cs - 必须包含所有前端地址
policy.WithOrigins(
    "http://localhost:3000",           // 本地开发
    "http://localhost:3001",           // 备用端口
    "http://18.183.240.121:3000",      // 生产环境
    "https://18.183.240.121:3000"      // HTTPS 支持
)
```

### API 路径配置一致性

| 环境 | 前端 API_BASE_URL | Nginx 代理 | 后端监听 | 最终路径 |
|------|-------------------|------------|----------|----------|
| **开发** | `http://localhost:5000/api` | - | `:5000` | 直连 |
| **生产** | `/api` | `backend:5000/api/` | `:5000` | 通过代理 |

---

## 🏗️ 最佳实践总结

### 1. 环境配置管理

**✅ 正确做法**:
```typescript
// 前端环境感知配置
const API_BASE_URL = process.env.NODE_ENV === 'production' 
  ? '/api'  // 生产环境使用相对路径
  : 'http://localhost:5000/api';  // 开发环境直连
```

**❌ 错误做法**:
```typescript
// 硬编码特定环境
const API_BASE_URL = 'http://localhost:5002/api';
```

### 2. 容器网络配置

**✅ 正确的 nginx 代理配置**:
```nginx
location /api/ {
    proxy_pass http://backend:5000/api/;  # 保持路径完整性
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
}
```

**❌ 错误的代理配置**:
```nginx
location /api/ {
    proxy_pass http://backend:5000/;  # 会截断 /api 路径
}
```

### 3. 安全组配置

**必需的端口开放**:
```
端口 22 (SSH)   - 管理访问
端口 3000 (前端) - 用户访问
端口 5000 (API)  - API 访问 (可选，通过前端代理)
```

### 4. CI/CD 环境检查

**✅ 适应性检查**:
```bash
# 根据实际环境调整要求
REQUIRED_SPACE=1048576  # 1GB 适合免费实例
# 自动清理释放空间
docker system prune -af 2>/dev/null || true
```

---

## 🔍 故障排除检查清单

### 当前端显示 "API 离线" 时

**1. 检查后端容器状态**
```bash
docker ps | grep backend
docker logs container_name
```

**2. 检查容器网络连通性**
```bash
# 从前端容器测试后端连接
docker exec frontend_container wget -qO- http://backend:5000/api/endpoint
```

**3. 检查 nginx 代理配置**
```bash
# 测试代理是否工作
curl http://localhost:3000/api/endpoint
```

**4. 检查 CORS 配置**
- 确保后端 CORS 包含前端地址
- 检查浏览器控制台的 CORS 错误

**5. 检查端口一致性**
- 前端 API_BASE_URL
- Docker compose 端口映射
- 安全组端口规则
- nginx 代理配置

### 配置文件同步检查

**关键配置对应关系**:
```
前端容器端口 (3000) ←→ 安全组规则 (3000) ←→ docker-compose ports (3000:80)
后端容器端口 (5000) ←→ 安全组规则 (5000) ←→ docker-compose ports (5000:5000)
前端 API_BASE_URL ←→ nginx proxy_pass ←→ 后端 CORS origins
```

---

## 📚 配置文件依赖关系图

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   AWS 安全组     │───▶│  Docker Compose │───▶│   应用配置       │
│  - 端口 3000    │    │  - ports: 3000  │    │  - CORS origins │
│  - 端口 5000    │    │  - ports: 5000  │    │  - API_BASE_URL │
│  - 端口 22      │    │  - 网络配置      │    │  - nginx proxy  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                        运行时环境                                │
│  - 网络可达性 (安全组)                                           │
│  - 端口映射 (Docker)                                            │
│  - 服务发现 (容器网络)                                           │
│  - 请求路由 (nginx + CORS)                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎯 核心经验总结

### 1. 配置一致性是关键
所有层级的配置必须保持一致：安全组 → Docker → 应用代码 → nginx

### 2. 环境感知配置
使用环境变量区分开发和生产环境，避免硬编码

### 3. 容器网络理解
容器内部使用服务名通信，外部使用 IP:端口访问

### 4. 渐进式故障排除
从网络层开始，逐步检查到应用层

### 5. 安全第一
永远不要提交敏感信息，使用专门的密钥管理服务

---

## 📖 相关文档链接

- [GitHub Secrets 配置指南](./docs/github-secrets-setup.md)
- [部署指南](./docs/deployment-guide.md)
- [快速开始](./SETUP-COMPLETE.md)

---

**💡 记住**: 每次部署问题都是学习机会，关键是建立系统性的故障排除流程和配置管理实践。