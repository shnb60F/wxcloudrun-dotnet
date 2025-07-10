using System.Threading.Tasks;
using aspnetapp;
using aspnetapp.EF;
using aspnetapp.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("api/[controller]")]
public class WebTestController : ControllerBase
{
    readonly JWTUtil jwtUtil;

    public WebTestController(JWTUtil jwtUtil)
    {
        this.jwtUtil = jwtUtil;
    }

    [HttpPost("resister")]
    public async Task<ActionResult> Resister([FromBody] TestResisterData resisterData)
    {
        string userId = Guid.NewGuid().ToString();

        string token = jwtUtil.Create(jwtBuilder =>
        {
            jwtBuilder.AddClaim("OpenId", userId);
            jwtBuilder.AddClaim("DeviceUniqueIdentifier", resisterData.DeviceUniqueIdentifier);
        });

        using GuardersContext guardersContext = new GuardersContext();
        GuarderDB guarderDB = new GuarderDB
        {
            GuarderKakera = 1000,
            GuarderLevel = 0,
            GuarderType = GuarderType.Archer,
            UserId = userId
        };
        UserDB userDB = new UserDB
        {
            UniqueId = resisterData.DeviceUniqueIdentifier,
            UserAP = 1000,
            UserCreatedAt = DateTime.Now,
            UserGold = 10000,
            UserId = userId,
            UserLevel = 0,
            UserName = resisterData.Name,
            UserXP = 0
        };
        Console.WriteLine($"Registering user: {userId}, Name: {resisterData.Name}, Device ID: {resisterData.DeviceUniqueIdentifier}");
        await guardersContext.Guarders.AddAsync(guarderDB);
        await guardersContext.Users.AddAsync(userDB);
        await guardersContext.SaveChangesAsync();

        JObject pairs = new JObject
        {
            ["Token"] = token
        };

        return Ok(pairs.ToString());
    }

    [HttpPost("login")]
    public ActionResult Login([FromBody] TestLoginData loginData)
    {
        try
        {
            // 解析 JWT token
            var tokenData = JObject.Parse(jwtUtil.Parse(loginData.Token));

            // 从解析的 token 中获取 OpenId
            string userId = $"{tokenData["OpenId"]}";
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("Invalid token: OpenId is missing.");
            }

            // 检查数据库中是否存在该用户（伪代码）
            bool userExists = CheckUserInDatabase(userId); // 伪代码：实现数据库检查逻辑
            if (!userExists)
            {
                return BadRequest("User not found.");
            }

            return Ok("Login successful.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Invalid token: {ex.Message}");
        }
    }

    private bool CheckUserInDatabase(string userId)
    {
        // 伪代码：检查数据库中是否存在用户
        // return database.Users.Any(u => u.UserId == userId);
        return true; // 假设用户存在
    }

    public class TestResisterData
    {
        public string DeviceUniqueIdentifier { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TestLoginData
    {
        public string Token { get; set; } = "";
        public string DeviceUniqueIdentifier { get; set; } = "";
    }
}
