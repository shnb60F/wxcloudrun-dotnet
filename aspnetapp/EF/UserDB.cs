using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspnetapp.EF;

public class UserDB
{
    [Key]
    public string UserId { get; set; }
    public string UserName { get; set; }
    public int UserXP { get; set; }
    public short UserLevel { get; set; }
    public long UserGold { get; set; }
    public short UserAP { get; set; }
    public DateTime UserCreatedAt { get; set; }
    public string UniqueId { get; set; }

    public virtual ICollection<GuarderDB> Guarders { get; set; }
    public virtual ICollection<QuestionDB> Questions { get; set; }
    public virtual ICollection<TreasureBoxDB> TreasureBoxs { get; set; }
}