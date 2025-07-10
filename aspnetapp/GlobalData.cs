namespace aspnetapp;

public static class GlobalData
{
    /// <summary>
    /// 设置JWT的秘钥
    /// </summary>
    public static readonly string Secret = "asnfifnbpoaerin";
    /// <summary>
    /// 小程序AppID
    /// </summary>
    public static readonly string AppId = "wx397f44bf841146f6";
    /// <summary>
    /// 小程序AppSecret
    /// </summary>
    public static readonly string AppSecret = "8d7ac3d42418752d8a4f3cf98a3e9f80";
    /// <summary>
    /// 微信登录状态接口地址
    /// </summary>
    public static readonly string Code2SessionUrl = "https://api.weixin.qq.com/sns/jscode2session";
}