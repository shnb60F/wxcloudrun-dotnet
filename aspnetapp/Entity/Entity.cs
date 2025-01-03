namespace aspnetapp.Entity;

public interface Entity : IEntity
{
    public float MaxHP { get; set; }
    public float MaxHPPercent { get; set; }
    public float HP { get; set; }
    public int Level { get; set; }
}