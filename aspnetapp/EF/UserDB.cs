using System.ComponentModel.DataAnnotations;

namespace aspnetapp.EF;

public class UserDB
{
    [Key]
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public int? UserXP { get; set; } = 0;
    public short? UserLevel { get; set; } = 0;
    public long? UserGold { get; set; } = 0;
    public short? UserAP { get; set; } = 10;
    public DateTime UserCreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<GuarderDB>? Guarders { get; set; }
    public virtual ICollection<QuestionDB>? Questions { get; set; }
    public virtual ICollection<TreasureBoxDB>? TreasureBoxs { get; set; }
}