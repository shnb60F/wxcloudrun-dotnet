namespace aspnetapp.Entity;

[Serializable]
public struct MainTower : Building
{
    public float AttankRange { get; set; }
    public float MaxHP { get; set; }
    public float HP { get; set; }
    public short Level { get; set; }
    public float MaxHPPercent { get; set; }

    public void AdjustForLevel(short level)
    {
        HP *= 1 + (float)level / 10;
    }
}