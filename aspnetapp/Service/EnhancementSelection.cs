
using aspnetapp.Entity;
using aspnetapp.Service;

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
    /// 弓箭手攻速强化10%
    /// </summary>
    ArcherASUp1p
}

public static class EnhancementSelection
{
    // "伤害强化": "强化伤害(Damage),无上限",
    // "穿透强化": "强化穿透(HP),无上限",
    // "射速强化": "降低冷却(CD),上限50%",
    // "多重射击": "增加释放次数,无上限"

    /// <summary>
    /// 强化,并且更新会话死线，状态
    /// </summary>
    /// <param name="guardersSession"></param>
    /// <param name="enhancementType"></param>
    public static void Enhancement(GuardersSession guardersSession, EnhancementType enhancementType)
    {
        switch (enhancementType)
        {
            case EnhancementType.ArcherDamageUp1p:
                foreach (dynamic guarder in guardersSession.entitys.Values)
                {
                    if (guarder is Archer)
                    {
                        guarder.Damage += 0.1f;
                    }
                }
                break;
            case EnhancementType.ArcherPiercingUp5p:
                foreach (dynamic guarder in guardersSession.entitys.Values)
                {
                    if (guarder is Archer)
                    {
                        guarder.Piercing += 0.5f;
                    }
                }
                break;
            case EnhancementType.ArcherASUp1p:
                foreach (dynamic guarder in guardersSession.entitys.Values)
                {
                    if (guarder is Archer)
                    {
                        guarder.SetCD(0.1f, false);
                    }
                }
                break;
            default:
                return;
        }
    }
}