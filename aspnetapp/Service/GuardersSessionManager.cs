using System.Collections.Concurrent;
using aspnetapp.Entity;
using Timer = System.Timers.Timer;

namespace aspnetapp.Service;

public class GuardersSessionManager
{
    readonly ConcurrentDictionary<(string, string), GuardersSession> sessions = new ConcurrentDictionary<(string, string), GuardersSession>();
    Timer timer;

    public GuardersSessionManager()
    {
        timer = new Timer(1000 * 60 * 6);
        timer.Elapsed += (sender, e) => ClearExpiredSessions();
        timer.AutoReset = true; // 确保计时器自动重置
        timer.Enabled = true; // 启用计时器
        timer.Start();
    }

    /// <summary>
    /// 添加一个会话
    /// </summary>
    /// <param name="key">UserId, QuestionID</param>
    public void AddSession((string, string) key, GuardersSession guardersSession)
    {
        if (sessions.ContainsKey(key))
        {
            sessions[key] = guardersSession;
        }
        else
        {
            sessions.AddOrUpdate(key, guardersSession, (k, existingValue) => guardersSession);
        }
    }

    public GuardersSession GetSession((string, string) key)
    {
        return sessions[key];
    }

    /// <summary>
    /// 完成会话，若客户端传来的会话与服务器端的会话一致则应用会话的强化
    /// </summary>
    /// <param name="key"></param>
    /// <param name="session"></param>
    /// <param name="enhancementType"></param>
    /// <exception cref="Exception"></exception>
    public bool SessionClear((string, string) key, GuardersSession session)
    {
        sessions[key].Enhancement();
        if (sessions[key].CompareEntities(session))
        {
            sessions[key] = session;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 记录强化
    /// </summary>
    /// <param name="enhancementType"></param>
    public void SetEnhancement((string, string) key, EnhancementType[] enhancementTypes)
    {
        sessions[key].SetEnhancement(enhancementTypes);
    }

    /// <summary>
    /// 植入当前强化，更新游标
    /// </summary>
    /// <param name="enhancementType"></param>
    public bool UpdateEnhancement((string, string) key, EnhancementType enhancementType)
    {
        return sessions[key].UpdateEnhancement(enhancementType);
    }

    public bool RemoveSession((string, string) key)
    {
        return sessions.TryRemove(key, out _);
    }

    void ClearExpiredSessions()
    {
        var expiredSessions = new List<(string, string)>();

        foreach (var session in sessions)
        {
            if (session.Value.DeadLine <= DateTime.Now)
            {
                expiredSessions.Add(session.Key);
            }
        }

        foreach (var key in expiredSessions)
        {
            RemoveSession(key);
        }
    }
}