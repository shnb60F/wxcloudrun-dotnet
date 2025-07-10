#!/bin/sh

# 等待数据库就绪（可选，视情况添加）
# echo "Waiting for database to be ready..."
# sleep 10

# 应用 EF Core 迁移
echo "Applying EF migrations..."
dotnet ef database update --verbose

# 启动应用程序
echo "Starting application..."
exec ./aspnetapp