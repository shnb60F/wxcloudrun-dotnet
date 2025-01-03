namespace aspnetapp.Entity;

public interface Creature : Entity
{
    public float AttankRange { get; set; }
    public float AttankRangePercent { get; set; }
    public float Speed { get; set; }
    public float SpeedPercent { get; set; }
    public float Damage { get; set; }
    public float DamagePercent { get; set; }
}