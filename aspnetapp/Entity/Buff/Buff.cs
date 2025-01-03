namespace aspnetapp.Entity;

/// <summary>
/// Entity只继承了Entity，不使用其字段
/// </summary>
public interface Buff : IEntity
{
    public float damage { get; set; }
    public Elements element { get; set; }
}