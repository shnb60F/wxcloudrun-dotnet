using System.Collections.Concurrent;
using Timer = System.Timers.Timer;

namespace aspnetapp.Service;

public class UserLoginService
{
    readonly ConcurrentDictionary<string, DateTime> users = new ConcurrentDictionary<string, DateTime>();
    Timer timer;

    public UserLoginService()
    {
        timer = new Timer(1000 * 60 * 6); // 每6分钟触发一次
        timer.Elapsed += (sender, e) => ClearExpiredUsers();
        timer.AutoReset = true; // 确保计时器自动重置
        timer.Enabled = true; // 启用计时器
        timer.Start(); // 启动计时器
    }

    // 记录用户登录信息
    public void AddUser(string user)
    {
        DateTime expirationTime = DateTime.Now.AddHours(1);
        users[user] = expirationTime;
    }

    // 移除用户登录信息
    public void RemoveUser(string user)
    {
        users.TryRemove(user, out _);
    }

    // 检查用户是否已登录
    public bool IsUserLoggedIn(string user)
    {
        if (users.ContainsKey(user))
        {
            DateTime expirationTime = DateTime.Now.AddHours(1);
            users[user] = expirationTime;
            return true;
        }
        return false;
    }

    // 清除已过期的登录用户
    void ClearExpiredUsers()
    {
        var expiredUsers = new List<string>();

        foreach (var user in users)
        {
            if (user.Value <= DateTime.Now)
            {
                expiredUsers.Add(user.Key);
            }
        }

        foreach (var user in expiredUsers)
        {
            RemoveUser(user);
        }
    }
}