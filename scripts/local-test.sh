#!/bin/bash
# scripts/local-test.sh
# 本地开发测试脚本

set -e

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}🧪 开始本地开发环境测试${NC}"
echo -e "${YELLOW}================================${NC}"

# 检查当前目录
if [ ! -f "README.md" ] || [ ! -d "frontend" ] || [ ! -d "backend" ]; then
    echo -e "${RED}❌ 请在项目根目录运行此脚本${NC}"
    exit 1
fi

# 检查端口占用
echo -e "${BLUE}🔍 检查端口占用情况...${NC}"
PORTS_TO_CHECK="3000 5000 5432"
for port in $PORTS_TO_CHECK; do
    if netstat -tuln | grep -q ":$port "; then
        echo -e "${YELLOW}⚠️  端口 $port 被占用${NC}"
        echo -n "是否停止占用端口的进程? (y/n): "
        read -r stop_process
        if [ "$stop_process" = "y" ] || [ "$stop_process" = "Y" ]; then
            PROCESS=$(lsof -ti:$port 2>/dev/null || true)
            if [ ! -z "$PROCESS" ]; then
                kill -9 $PROCESS 2>/dev/null || true
                echo -e "${GREEN}✅ 端口 $port 已释放${NC}"
            fi
        fi
    else
        echo -e "${GREEN}✅ 端口 $port 可用${NC}"
    fi
done

# 启动开发环境
echo -e "${BLUE}🚀 启动本地开发环境...${NC}"

# 检查 podman-compose
if command -v podman-compose &> /dev/null; then
    echo -e "${GREEN}🐳 使用 podman-compose 启动服务${NC}"
    podman-compose -f docker-compose.podman.yml up -d
elif command -v docker-compose &> /dev/null; then
    echo -e "${GREEN}🐳 使用 docker-compose 启动服务${NC}"
    # 设置 podman socket 兼容
    export DOCKER_HOST=unix:///run/user/$UID/podman/podman.sock
    systemctl --user start podman.socket 2>/dev/null || true
    docker-compose -f docker-compose.podman.yml up -d
else
    echo -e "${YELLOW}⚠️  未找到 compose 工具，手动启动容器...${NC}"
    
    # 创建网络
    podman network exists trade-network || podman network create trade-network
    
    # 启动 PostgreSQL
    echo -e "${BLUE}📦 启动数据库...${NC}"
    podman run -d --name dev-postgres \
        --network trade-network \
        -e POSTGRES_DB=trademanagement \
        -e POSTGRES_USER=postgres \
        -e POSTGRES_PASSWORD=postgres123 \
        -p 5432:5432 \
        postgres:15-alpine 2>/dev/null || echo "数据库容器已存在"
    
    # 等待数据库启动
    sleep 5
    
    echo -e "${YELLOW}💡 请手动启动前端和后端:${NC}"
    echo "后端: cd backend/TradeManagementApi && dotnet run"
    echo "前端: cd frontend/trade-management && npm start"
fi

# 等待服务启动
echo -e "${BLUE}⏳ 等待服务启动...${NC}"
sleep 10

# 测试服务
echo -e "${BLUE}🧪 测试服务连接...${NC}"

# 测试数据库
echo -n "数据库 (5432): "
if nc -z localhost 5432 2>/dev/null; then
    echo -e "${GREEN}✅ 可访问${NC}"
else
    echo -e "${RED}❌ 不可访问${NC}"
fi

# 测试后端 API
echo -n "后端 API (5000): "
if curl -s http://localhost:5000/api/tradedocuments >/dev/null 2>&1; then
    echo -e "${GREEN}✅ 可访问${NC}"
else
    echo -e "${RED}❌ 不可访问${NC}"
fi

# 测试前端
echo -n "前端服务 (3000): "
if curl -s http://localhost:3000 >/dev/null 2>&1; then
    echo -e "${GREEN}✅ 可访问${NC}"
else
    echo -e "${RED}❌ 不可访问${NC}"
fi

echo -e "${GREEN}🎉 本地开发环境设置完成！${NC}"
echo -e "${YELLOW}📋 访问地址:${NC}"
echo -e "${BLUE}  - 前端应用: http://localhost:3000${NC}"
echo -e "${BLUE}  - 后端 API: http://localhost:5000${NC}"
echo -e "${BLUE}  - Swagger: http://localhost:5000/swagger${NC}"
echo ""
echo -e "${YELLOW}🔧 开发工作流程:${NC}"
echo -e "${BLUE}1. 修改代码${NC}"
echo -e "${BLUE}2. 在浏览器中测试功能${NC}"
echo -e "${BLUE}3. 确认无误后提交: git add . && git commit && git push${NC}"
echo ""
echo -e "${YELLOW}🛑 停止服务: podman-compose -f docker-compose.podman.yml down${NC}"