namespace aspnetapp.Entity;

[Serializable]
public struct MainTower : Building
{
    public float AttankRange { get; set; }
    public float MaxHP { get; set; }
    public float HP { get; set; }
    public int Level { get; set; }
    public float MaxHPPercent { get; set; }
}