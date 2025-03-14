namespace aspnetapp.Service;

using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using aspnetapp.Entity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GuardersSession
{
    /// <summary>
    /// 用户ID(init param)
    /// </summary>
    public string UserId { get; set; } = "";
    /// <summary>
    /// 以N-或E-开头，N-表示普通关卡，E-表示无尽关卡(init param)
    /// </summary>
    public string QuestionID { get; set; } = "";
    /// <summary>
    /// 销毁时间(init param)
    /// </summary>
    public DateTime DeadLine { get; set; }
    public long TimeFrame { get; set; }
    /// <summary>
    /// 已经确定的强化
    /// </summary>
    public List<EnhancementType> enhancements { get; } = new List<EnhancementType>();
    /// <summary>
    /// 待确定的强化
    /// </summary>
    EnhancementType[] tempEnhancements = new EnhancementType[3];
    /// <summary>
    /// 是否为待确定状态
    /// </summary>
    public bool isReady { get; private set; } = false;

    /// <summary>
    /// 需要监控的重要实体
    /// </summary>
    public Dictionary<string, IEntity> entitys { get; } = new Dictionary<string, IEntity>();

    static Assembly assembly = Assembly.GetExecutingAssembly();
    /// <summary>
    /// 用于记录强化数量；Init时重置剩余可强化集合（初始化时生成可强化集合，每次更新强化时更改；可返回剩余可强化集合），将被废弃
    /// </summary>
    [Obsolete] readonly Dictionary<EnhancementType, int> _enhancementCounts = new Dictionary<EnhancementType, int>();
    /// <summary>
    /// 可强化集合，需要获取Count时获取
    /// </summary>
    public List<EnhancementType> ReinforceableSet { get; private set; } = new List<EnhancementType>();
    /// <summary>
    /// 用于ReinforceableSet计数
    /// </summary>
    public int ReinforceableSetLength { get; private set; } = 0;

    public GuardersSession()
    {

    }

    /// <summary>
    /// 目前主要用途为初始化可强化集合
    /// </summary>
    public void Init()
    {
        foreach (var entity in entitys.Values)
        {
            if (entity is Archer)
            {
                switch (((Archer)entity).Level)
                {
                    case 0:
                        ReinforceableSet.Add(EnhancementType.ArcherAIDown1p);
                        ReinforceableSet.Add(EnhancementType.ArcherDamageUp1p);
                        ReinforceableSet.Add(EnhancementType.ArcherPiercingUp5p);
                        ReinforceableSetLength += 3;
                        continue;
                    case 1:
                        ReinforceableSet.Add(EnhancementType.ArcherShoutCountUp10p);
                        ReinforceableSetLength++;
                        continue;
                    default:
                        break;
                }
            }
        }
    }

    public void ReadJson(string json)
    {
        JObject jObject = JObject.Parse(json);

        if (jObject == null)
            return;
        foreach (var item in jObject["entitys"].OfType<JProperty>())
        {
            string key = item.Name;
            string className = $"aspnetapp.Entity.{item.Value["Type"]}";
            // 根据类名获取类型信息
            Type? type = assembly.GetType(className);
            if (type == null)
                throw new Exception($"Type '{className}' not found.");
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
            return false;

        foreach (var kvp in entitys)
        {
            if (!other.entitys.TryGetValue(kvp.Key, out var otherEntity) || !kvp.Value.Equals(otherEntity))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 记录强化
    /// </summary>
    /// <param name="enhancementType"></param>
    public void SetEnhancement(EnhancementType[] enhancementTypes)
    {
        isReady = true;
        tempEnhancements = enhancementTypes;
    }

    /// <summary>
    /// 植入当前强化，更新游标
    /// </summary>
    /// <param name="enhancementType"></param>
    public bool UpdateEnhancement(EnhancementType enhancementType)
    {
        if (!isReady)
            return false;
        foreach (var item in tempEnhancements)
        {
            if (enhancementType == item)
            {
                DeadLine = DateTime.Now.AddMinutes(60);
                enhancements.Add(enhancementType);
                //若抵达上限，则从可强化集合中删除，以避免再次生成此强化
                switch (enhancementType)
                {
                    case EnhancementType.ArcherAIDown1p:
                        ReinforceableSet.Remove(EnhancementType.ArcherAIDown1p);
                        ReinforceableSetLength--;
                        break;
                    default:
                        break;
                }
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

    /// <summary>
    /// 获取强化叠加数量
    /// </summary>
    /// <param name="enhancementType"></param>
    /// <returns></returns>
    public int GetEnhancementCount(EnhancementType enhancementType)
    {
        if (_enhancementCounts.TryGetValue(enhancementType, out int count))
        {
            return count;
        }
        return 0;
    }

    public void AddEnhancement(EnhancementType enhancementType)
    {
        if (_enhancementCounts.ContainsKey(enhancementType))
        {
            _enhancementCounts[enhancementType]++;
        }
        else
        {
            _enhancementCounts[enhancementType] = 1;
        }
    }
}