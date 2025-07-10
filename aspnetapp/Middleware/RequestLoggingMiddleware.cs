using Newtonsoft.Json.Linq;
using System.Text;
using aspnetapp.Utils;
using Microsoft.EntityFrameworkCore;
using aspnetapp.Service;

namespace aspnetapp.Middleware;

public class RequestLoggingMiddleware
{
    readonly RequestDelegate _next;
    readonly ILogger<RequestLoggingMiddleware> _logger;
    readonly JWTUtil jwtUtil;
    readonly UserLoginService userLoginService;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, JWTUtil jwtUtil, UserLoginService userLoginService)
    {
        _next = next;
        _logger = logger;
        this.jwtUtil = jwtUtil;
        this.userLoginService = userLoginService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 在这里可以处理请求前的逻辑
        _logger.LogInformation($"{DateTime.Now}: Incoming request: {context.Request.Method} {context.Request.Path}\n");

        if (context.Request.Path.StartsWithSegments("/api/Wechat/login", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }
        else if (context.Request.Path.StartsWithSegments("/api/WebTest", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }
        else if (context.Request.Path.StartsWithSegments("/api/Hello", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }
        else if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
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

        // 解析请求体中 token 键的值
        JToken token;
        if (!requestBodyJson.TryGetValue("token", out token))
        {
            // 如果请求体中没有 token 键，可以返回 401 状态码
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token is required.");
            return;
        }

        // 解析 token 值
        JObject tokenJson = JObject.Parse(jwtUtil.Parse(token.ToString()));

        // 获取 userId 键的值
        JToken userId;
        if (!tokenJson.TryGetValue("OpenId", out userId))
        {
            await _next(context);
            return;
        }

        // 用户短时间内已经登录，则不查询数据库
        if (userLoginService.IsUserLoggedIn($"{userId}"))
        {
            context.Request.Headers.Add("UserId", $"{userId}");
            await _next(context);
            return;
        }

        // 查询数据库
        using var guardersContext = new GuardersContext();
        var user = await guardersContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == $"{userId}");

        if (user == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("User not found.");
            return;
        }

        // 向响应头添加 UserId
        context.Request.Headers.Add("UserId", $"{userId}");
        userLoginService.AddUser($"{userId}");

        // 调用下一个中间件
        await _next(context);

        // 在这里可以处理响应后的逻辑
        _logger.LogInformation($"{DateTime.Now} {userId}: Outgoing response: {context.Response.StatusCode}\n");
    }
}