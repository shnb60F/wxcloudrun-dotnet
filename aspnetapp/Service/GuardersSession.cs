namespace aspnetapp.Service;

using System.Reflection;
using aspnetapp.Entity;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GuardersSession
{
    /// <summary>
    /// 用户ID(init param)
    /// </summary>
    public string UserId { get; set; }
    /// <summary>
    /// 以N-或E-开头，N-表示普通关卡，E-表示无尽关卡(init param)
    /// </summary>
    public string QuestionID { get; set; }
    /// <summary>
    /// 销毁时间(init param)
    /// </summary>
    public DateTime DeadLine { get; set; }
    public long TimeFrame { get; set; }
    /// <summary>
    /// 进度只能在EnhancementSelection中修改
    /// </summary>
    public List<EnhancementType> enhancements { get; } = new List<EnhancementType>();

    public Dictionary<string, IEntity> entitys { get; } = new Dictionary<string, IEntity>();

    public void ReadJson(string json)
    {
        JObject jObject = JObject.Parse(json);
        // 获取程序集
        Assembly assembly = Assembly.GetExecutingAssembly();

        foreach (var item in jObject["entitys"].OfType<JProperty>())
        {
            string key = item.Name;
            string className = $"aspnetapp.Entity.{item.Value["Type"]}";
            // 根据类名获取类型信息
            Type type = assembly.GetType(className);
            if (type == null)
            {
                throw new Exception($"Type '{className}' not found.");
            }
            IEntity entity = (IEntity)JsonConvert.DeserializeObject(item.Value.ToString(), type);
            entitys.Add(key, entity);
        }
    }

    public void AddEntity(string key, IEntity entity)
    {
        if (entitys.ContainsKey(key))
        {
            entitys[key] = entity; // 更新值
        }
        else
        {
            entitys.Add(key, entity); // 添加新值
        }
    }

    public IEntity? GetEntity(string key)
    {
        if (entitys.ContainsKey(key))
        {
            return entitys[key];
        }
        return null;
    }

    public bool RemoveEntity(string key)
    {
        return entitys.Remove(key);
    }

    public bool CompareEntities(GuardersSession other)
    {
        if (other == null || entitys.Count != other.entitys.Count)
        {
            return false;
        }

        foreach (var kvp in entitys)
        {
            if (!other.entitys.TryGetValue(kvp.Key, out var otherEntity) || !kvp.Value.Equals(otherEntity))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 应用强化
    /// </summary>
    /// <param name="session"></param>
    /// <param name="enhancementType"></param>
    /// <returns></returns>
    public void Enhancement()
    {
        if (enhancements.Count > 20)
            throw new Exception("Enhancement count should be less than 20.");
        foreach (var enhancementType in enhancements)
        {
            EnhancementSelection.Enhancement(this, enhancementType);
        }
    }
}
