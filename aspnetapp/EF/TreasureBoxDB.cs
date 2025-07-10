using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aspnetapp.EF;

public class TreasureBoxDB
{
    [Key]
    public string TreasureBoxId { get; set; }
    public TreasureBoxType TreasureBoxType { get; set; }
    public DateTime TreasureBoxCreateAt { get; set; }

    public string UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual UserDB User { get; set; }
}

public enum TreasureBoxType
{
    /// <summary>
    /// 普通
    /// </summary>
    Common,
    /// <summary>
    /// 稀有
    /// </summary>
    Rare,
    /// <summary>
    /// 史诗
    /// </summary>
    Epic,
    /// <summary>
    /// 传说
    /// </summary>
    Legendary
}