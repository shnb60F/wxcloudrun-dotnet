using Timer = System.Timers.Timer;

namespace aspnetapp.Service;

public class GuardersSessionManager
{
    readonly Dictionary<(string, string), GuardersSession> sessions = new Dictionary<(string, string), GuardersSession>();
    Timer timer;

    public GuardersSessionManager()
    {
        timer = new Timer(1000 * 60 * 6);
        timer.Elapsed += (sender, e) =>
        {
            foreach (var session in sessions)
            {
                if (session.Value.DeadLine > DateTime.Now)
                {
                    RemoveSession(session.Key);
                }
            }
        };
        timer.Enabled = true;
        timer.Start();
    }

    /// <summary>
    /// 添加一个会话
    /// </summary>
    /// <param name="key">UserId, QuestionID</param>
    public void AddSession((string, string) key)
    {
        if (sessions.ContainsKey(key))
        {
            sessions[key] = new GuardersSession { DeadLine = DateTime.Now.AddMinutes(60), UserId = key.Item1, QuestionID = key.Item2, TimeFrame = 0 };
        }
        else
        {
            sessions.Add(key, new GuardersSession { DeadLine = DateTime.Now.AddMinutes(60), UserId = key.Item1, QuestionID = key.Item2, TimeFrame = 0 });
        }
    }

    public GuardersSession GetSession((string, string) key)
    {
        return sessions[key];
    }

    /// <summary>
    /// 更新会话，若客户端传来的会话与服务器端的会话一致则更新会话，不一致则抛出异常
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

    bool RemoveSession((string, string) key)
    {
        return sessions.Remove(key);
    }
}