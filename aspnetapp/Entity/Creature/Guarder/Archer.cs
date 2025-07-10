namespace aspnetapp.Entity;

[Serializable]
public struct Archer : Guarder
{
    public float CD { get; set; }
    public float CDPercent { get; set; }
    public float CDing { get; set; }
    public float AttankRange { get; set; }
    public float AttankRangePercent { get; set; }
    /// <summary>
    /// 射击次数
    /// </summary>
    public float ShoutCount { get; set; }
    /// <summary>
    /// 穿透
    /// </summary>
    public float Piercing { get; set; }
    public float Speed { get; set; }
    public float SpeedPercent { get; set; }
    public float Damage { get; set; }
    public float DamagePercent { get; set; }
    public float MaxHP { get; set; }
    public float MaxHPPercent { get; set; }
    public float HP { get; set; }
    public short Level { get; set; }

    public Archer()
    {
        CD = 5;
        CDPercent = 1;
        CDing = 0;
        AttankRange = 50;
        AttankRangePercent = 1;
        ShoutCount = 1;
        Piercing = 1;
        Speed = 4.2f;
        SpeedPercent = 1;
        Damage = 1;
        DamagePercent = 1;
        MaxHP = 10;
        MaxHPPercent = 1;
        HP = MaxHP;
        Level = 0;
    }

    public void AdjustForLevel(short level)
    {
        HP *= 1 + (float)level / 10;
    }
}