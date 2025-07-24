# 🔧 故障排除快速指南

## 🚨 常见问题速查表

### 问题：前端显示 "API 离线"

**症状**: 前端加载但显示 API 连接失败
**可能原因**: API 端点配置、CORS、网络连通性

**检查步骤**:
```bash
# 1. 检查容器状态
docker ps

# 2. 检查后端日志
docker logs trade-demo-deploy_backend_1

# 3. 测试 API 直接访问
curl http://localhost:5000/api/tradedocuments

# 4. 测试 nginx 代理
curl http://localhost:3000/api/tradedocuments

# 5. 检查浏览器网络面板
# 查看具体的 HTTP 错误码
```

### 问题：部署时磁盘空间不足

**症状**: `❌ Insufficient disk space`
**解决方案**:
```bash
# 清理 Docker
docker system prune -af

# 清理包缓存
sudo apt-get clean || sudo yum clean all

# 清理临时文件
sudo rm -rf /tmp/*

# 检查空间
df -h
```

### 问题：GitHub 推送被拒绝

**症状**: `Push cannot contain secrets`
**解决方案**:
```bash
# 移除敏感文件
git rm --cached sensitive-file.txt
echo "sensitive-file.txt" >> .gitignore

# 重写历史 (谨慎使用)
git reset --hard HEAD~1
```

---

## 📋 配置检查清单

### ✅ 端口配置一致性检查

- [ ] 安全组开放了正确端口 (22, 3000, 5000)
- [ ] docker-compose 端口映射正确
- [ ] 前端 API_BASE_URL 配置正确
- [ ] 后端 CORS 包含所有前端地址
- [ ] nginx 代理路径配置正确

### ✅ 网络连通性检查

- [ ] 容器之间可以通过服务名通信
- [ ] 外部可以访问映射的端口
- [ ] nginx 可以代理到后端服务
- [ ] CORS 头设置正确

### ✅ 环境配置检查

- [ ] GitHub Secrets 配置完整
- [ ] 环境变量正确传递到容器
- [ ] 生产环境 vs 开发环境配置区分
- [ ] SSL 证书配置 (如需要)

---

## 🔍 高级故障排除

### 容器网络调试

```bash
# 查看容器网络
docker network ls
docker network inspect trade-demo-deploy_trade-network

# 进入容器调试
docker exec -it container_name /bin/sh

# 测试容器间连通性
docker exec frontend_container ping backend
docker exec frontend_container wget -qO- http://backend:5000/api/health
```

### 应用层调试

```bash
# 查看详细日志
docker logs --follow --tail 100 container_name

# 检查配置文件
docker exec container_name cat /path/to/config

# 检查进程状态
docker exec container_name ps aux
docker exec container_name netstat -tlnp
```

---

## 📞 获取帮助

遇到问题时：
1. 查看本指南的相关章节
2. 检查 [LESSONS-LEARNED.md](./LESSONS-LEARNED.md)
3. 查看项目 Issues: https://github.com/ocean5tech/trade-demo/issues
4. 参考官方文档

---

**🚀 提示**: 保存此文档的链接，在部署时随时参考！