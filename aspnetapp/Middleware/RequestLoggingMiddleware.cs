using Newtonsoft.Json.Linq;
using System.Text;
using aspnetapp.Utils;
using Microsoft.EntityFrameworkCore;

namespace aspnetapp.Middleware;

public class RequestLoggingMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<RequestLoggingMiddleware> _logger;
    readonly JWTUtil jwtUtil;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, JWTUtil jwtUtil)
    {
        _next = next;
        _logger = logger;
        this.jwtUtil = jwtUtil;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 在这里可以处理请求前的逻辑
        _logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

        if (context.Request.Path.StartsWithSegments("/api/Wechat/login", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // 读取请求体
        context.Request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
        await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
        var requestBody = Encoding.UTF8.GetString(buffer);
        context.Request.Body.Position = 0;

        // 将请求体转换为 JObject
        JObject requestBodyJson = JObject.Parse(requestBody);

        // 解析 token 键的值
        if (requestBodyJson.TryGetValue("token", out JToken token))
        {
            // 解析 token 值
            JObject tokenJson = JObject.Parse(jwtUtil.Parse(token.ToString()));
            if (tokenJson.TryGetValue("OpenId", out JToken userId))
            {
                //
                using var guardersContext = new GuardersContext();
                var user = await guardersContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == $"{userId}");

                if (user == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("User not found.");
                    return;
                }
                // 向响应头添加 UserId
                context.Request.Headers.Add("UserId", userId.ToString());
            }
        }
        else
        {
            // 如果请求体中没有 token 键，可以返回 401 状态码
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token is required.");
            return;
        }

        // 调用下一个中间件
        await _next(context);

        // 在这里可以处理响应后的逻辑
        _logger.LogInformation($"Outgoing response: {context.Response.StatusCode}");
    }
}