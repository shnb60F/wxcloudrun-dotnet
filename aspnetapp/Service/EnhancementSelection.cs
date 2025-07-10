using aspnetapp.EF;
using aspnetapp.Entity;

namespace aspnetapp.Service;

public enum EnhancementType
{
    /// <summary>
    /// 弓箭手伤害强化10%
    /// </summary>
    ArcherDamageUp1p,
    /// <summary>
    /// 弓箭手穿透强化50%
    /// </summary>
    ArcherPiercingUp5p,
    /// <summary>
    /// 弓箭手攻击间隔（Attack Interval）减少10%
    /// </summary>
    ArcherAIDown1p,
    /// <summary>
    /// 弓箭手攻击次数增加100%
    /// </summary>
    ArcherShoutCountUp10p,
    /// <summary>
    /// 解锁火法师
    /// </summary>
    FireHouShiUnLock,
    /// <summary>
    /// 弓箭手伤害强化10%
    /// </summary>
    FireHouShiDamageUp1p,
}

/// <summary>
/// 应用强化功能
/// </summary>
public static class EnhancementSelection
{
    // "伤害强化": "强化伤害(Damage),无上限",
    // "穿透强化": "强化穿透(HP),无上限",
    // "射速强化": "降低冷却(CD),上限50%",
    // "多重射击": "增加释放次数,无上限"

    /// <summary>
    /// 应用强化
    /// </summary>
    /// <param name="guardersSession"></param>
    /// <param name="enhancementType"></param>
    public static void Enhancement(List<GuarderDB> guarderDBs, List<EnhancementType> enhancements)
    {
        List<IEntity> entitys = new List<IEntity>();
        foreach (var guarder in guarderDBs)
        {
            switch (guarder.GuarderType)
            {
                case GuarderType.Archer:
                    entitys.Add(new Archer());
                    break;
                case GuarderType.FireHoushi:
                    entitys.Add(new FireHoushi());
                    break;
            }
        }

        foreach (var enhancement in enhancements)
        {
            switch (enhancement)
            {
                case EnhancementType.ArcherDamageUp1p:
                    foreach (dynamic guarder in entitys)
                    {
                        if (guarder.GuarderType is Archer)
                        {
                            guarder.Damage += 0.1f;
                        }
                    }
                    break;
                case EnhancementType.ArcherPiercingUp5p:
                    foreach (dynamic guarder in entitys)
                    {
                        if (guarder is Archer)
                        {
                            guarder.Piercing += 0.5f;
                        }
                    }
                    break;
                case EnhancementType.ArcherAIDown1p:
                    foreach (dynamic guarder in entitys)
                    {
                        if (guarder is Archer)
                        {
                            guarder.CDPercent -= 0.1f;
                        }
                    }
                    break;
                case EnhancementType.ArcherShoutCountUp10p:
                    foreach (dynamic guarder in entitys)
                    {
                        if (guarder is Archer)
                        {
                            guarder.ShoutCount += 1;
                        }
                    }
                    break;
            }
        }
    }
}