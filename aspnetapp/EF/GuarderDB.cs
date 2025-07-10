using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace aspnetapp.EF;

public class GuarderDB
{
    /// <summary>
    /// 与GuarderType为混合主键
    /// </summary>
    public string UserId { get; set; }
    /// <summary>
    /// 与UserId为混合主键
    /// </summary>
    public GuarderType GuarderType { get; set; }
    public short GuarderLevel { get; set; }
    public int GuarderKakera { get; set; }

    [ForeignKey("UserId")]
    public virtual UserDB User { get; set; }
}

public enum GuarderType
{
    Archer,
    FireHoushi
}