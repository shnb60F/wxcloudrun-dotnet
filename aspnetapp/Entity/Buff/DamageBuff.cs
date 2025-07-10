namespace aspnetapp.Entity;

[Serializable]
public struct DamageBuff : Buff
{
    public float damage { get; set; }
    public Elements element { get; set; }
}