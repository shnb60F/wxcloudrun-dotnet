using JWT.Algorithms;
using JWT.Builder;

namespace aspnetapp.Utils;

public class JWTUtil
{
    public string Create(Action<JwtBuilder>? action = null, string secret = "")
    {
        secret = GlobalData.Secret;
        JwtBuilder jwtBuilder = new JwtBuilder()
                                    .WithAlgorithm(new HMACSHA256Algorithm()) // 设置加密算法
                                    .WithSecret(secret) // 设置秘钥
                                    .AddClaim("exp", DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds());
        if (action != null)
            action(jwtBuilder);
        return jwtBuilder.Encode(); // 生成JWT字符串
    }

    public string Parse(string token, string secret = "")
    {
        secret = GlobalData.Secret;
        return new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(secret) // 设置秘钥
                    .MustVerifySignature()
                    .Decode(token);
    }
}