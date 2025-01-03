namespace aspnetapp.Entity;

[Serializable]
public struct HouShi : Guarder
{
    public float CD { get; set; }
    public float CDing { get; set; }
    public float AttankRange { get ; set; }
    public float Speed { get ; set; }
    public float Damage { get ; set; }
    public float MaxHP { get ; set; }
    public float HP { get ; set; }
    public int Level { get ; set; }
    public float AttankRangePercent { get ; set; }
    public float SpeedPercent { get ; set; }
    public float DamagePercent { get ; set; }
    public float MaxHPPercent { get ; set; }
}