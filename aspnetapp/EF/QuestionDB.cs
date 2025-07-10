using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using aspnetapp.EF;

public class QuestionDB
{
    /// <summary>
    /// 与QuestionID为混合主键
    /// </summary>
    public string UserId { get; set; }
    /// <summary>
    /// 与UserId为混合主键
    /// </summary>
    public string QuestionID { get; set; }
    public short QuestionStar { get; set; }
    public int QuestionBestTime { get; set; }

    [ForeignKey("UserId")]
    public virtual UserDB? User { get; set; }
}