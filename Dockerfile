# ===== 构建阶段 =====
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build

# 替换为腾讯云镜像加速构建
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.tencent.com/g' /etc/apk/repositories

WORKDIR /source

# 拷贝项目和恢复依赖
COPY *.sln .
COPY aspnetapp/*.csproj ./aspnetapp/
RUN dotnet restore -r linux-musl-x64 /p:PublishReadyToRun=true

# 拷贝代码并发布应用（单文件 + 自包含）
COPY aspnetapp/. ./aspnetapp/
WORKDIR /source/aspnetapp
RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained true \
    --no-restore --no-build /p:PublishTrimmed=true \
    /p:PublishReadyToRun=true /p:PublishSingleFile=true

# ===== 运行阶段 =====
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine

# 替换为腾讯云镜像
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.tencent.com/g' /etc/apk/repositories

# 安装证书以支持 HTTPS
RUN apk add --no-cache ca-certificates

# 设置工作目录
WORKDIR /app

# 拷贝构建输出
COPY --from=build /app ./

# 可选：设置中文支持和时区
# RUN apk add --no-cache tzdata icu-libs && \
#     cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && \
#     echo "Asia/Shanghai" > /etc/timezone

# 启动脚本（先迁移再运行）
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

ENTRYPOINT ["./entrypoint.sh"]