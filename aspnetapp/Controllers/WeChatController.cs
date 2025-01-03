using System.Threading.Tasks.Dataflow;
using aspnetapp;
using aspnetapp.EF;
using aspnetapp.Service;
using aspnetapp.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("api/[controller]")]
public class WeChatController : ControllerBase
{
    readonly GuardersService guardersService;
    readonly GuardersSessionManager guardersSessionManager;
    readonly JWTUtil jwtUtil;

    public WeChatController(GuardersService guardersService, GuardersSessionManager guardersSessionManager, JWTUtil jwtUtil)
    {
        this.guardersService = guardersService;
        this.guardersSessionManager = guardersSessionManager;
        this.jwtUtil = jwtUtil;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginData loginData)
    {
        if (string.IsNullOrEmpty(loginData.Code) || string.IsNullOrEmpty(loginData.Name))
        {
            return BadRequest("Code or Name is required.");
        }

        using var client = new HttpClient();
        var response = await client.GetAsync($"{GlobalData.Code2SessionUrl}?appid={GlobalData.AppId}&secret={GlobalData.AppSecret}&js_code={loginData.Code}&grant_type=authorization_code");

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Failed to exchange code for session.");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        JObject data = JObject.Parse(responseContent);

        if (data["errcode"] != null)
        {
            return StatusCode(500, $"Error from WeChat: {data["errmsg"]}");
        }

        var openId = $"{data["openid"]}";
        var sessionKey = $"{data["session_key"]}";

        await guardersService.Login(openId, loginData.Name);

        // 在这里你可以将 openId 和 sessionKey 存储到数据库或会话中，
        // 并返回给客户端一些必要的信息，比如自定义 token 等。
        // 为了安全起见，不要直接返回 session_key 给客户端。

        //以openid为负载的 JWT token
        var token = jwtUtil.Create((jwtBuilder) => { jwtBuilder.AddClaim("OpenId", openId); });

        return Ok(new
        {
            Token = token,
            // 可选：返回其他需要的信息
        });
    }

    [HttpPost("logoff")]
    public async Task<IActionResult> Logoff([FromBody] LogOffData logOffData, [FromHeader] string userId)
    {
        if (await guardersService.Logoff($"{userId}"))
        {
            return Ok();
        }
        else
        {
            return BadRequest("User not found.");
        }
    }

    [HttpPost("guarder/create")]
    public async Task<IActionResult> GuarderCreate([FromBody] GuarderCreateData guarderCreateData, [FromHeader] string userId)
    {
        GuarderType guarderType = guarderCreateData.GuarderType;

        if (await guardersService.GuarderCreate(userId, guarderType))
        {
            return Ok();
        }
        else
        {
            return BadRequest("Failed to create guarder.");
        }
    }

    [HttpPost("guarder/levelup")]
    public async Task<IActionResult> GuarderLevelUp([FromBody] GuarderLevelUpData guarderLevelUpData, [FromHeader] string userId)
    {
        if (await guardersService.GuarderLevelUp(userId, guarderLevelUpData.GuarderType))
        {
            return Ok();
        }
        else
        {
            return BadRequest("Failed to level up guarder.");
        }
    }

    [HttpPost("question/create")]
    public IActionResult QuestionCreate([FromBody] QuestionCreateData questionCreateData, [FromHeader] string userId)
    {
        string questionID = questionCreateData.QuestionID;

        guardersSessionManager.AddSession((userId, questionID));
        return Ok();
    }

    [HttpPost("question/getenhancement")]
    public IActionResult QuestionGetEnhancement([FromBody] QuestionGetEnhancementData questionGetEnhancementData, [FromHeader] string userId)
    { 
        return Ok(EnhancementSelectionService.GetEnhancement());
    }

    [HttpPost("question/enhancement")]
    public IActionResult QuestionEnhancement([FromBody] QuestionEnhancementData questionEnhancementData, [FromHeader] string userId)
    {
        string questionID = questionEnhancementData.QuestionID;
        EnhancementType enhancementType = questionEnhancementData.EnhancementType;

        guardersSessionManager.GetSession((userId, questionID)).enhancements.Add(enhancementType);
        return Ok();
    }

    [HttpPost("question/clear")]
    public async Task<IActionResult> QuestionClear([FromBody] QuestionClearData questionClearData, [FromHeader] string userId)
    {
        string questionID = questionClearData.QuestionID;
        string data = questionClearData.Data;
        long timeFrame = questionClearData.TimeFrame;

        GuardersSession guardersSession = new GuardersSession { UserId = userId, QuestionID = questionID, TimeFrame = timeFrame };
        guardersSession.ReadJson(data);

        //UpdateSession是否更新成功
        if (guardersSessionManager.SessionClear((userId, questionID), guardersSession))
        {
            await guardersService.QuestionClear(
                new QuestionDB { UserId = userId, QuestionID = questionID },
            new TreasureBoxDB { TreasureBoxId = $"{Guid.NewGuid()}", TreasureBoxType = TreasureBoxType.Rare, UserId = userId }, 1);
            return Ok();
        }
        else
        {
            return BadRequest("Failed to clear question.");
        }
    }

    [HttpPost("user/levelup")]
    public async Task<IActionResult> UserLevelUp([FromBody] UserLevelUpData userLevelUpData, [FromHeader] string userId)
    {
        if (await guardersService.UserLevelUp(userId))
        {
            return Ok();
        }
        else
        {
            return BadRequest("Failed to level up user.");
        }
    }
}

public class LoginData
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public class LogOffData
{
    public string Token { get; set; } = "";
}

public class GuarderCreateData
{
    public string Token { get; set; } = "";
    public GuarderType GuarderType { get; set; }
}

public class GuarderLevelUpData
{
    public string Token { get; set; } = "";
    public GuarderType GuarderType { get; set; }
}

public class QuestionCreateData
{
    public string Token { get; set; } = "";
    public string QuestionID { get; set; } = "";
}

public class QuestionGetEnhancementData
{
    public string Token { get; set; } = "";
    public string QuestionID { get; set; } = "";
}

public class QuestionEnhancementData
{
    public string Token { get; set; } = "";
    public string QuestionID { get; set; } = "";
    public EnhancementType EnhancementType { get; set; }
}

public class QuestionClearData
{
    public string Token { get; set; } = "";
    /// <summary>
    /// 以N-或E-开头，N-表示普通关卡，E-表示无尽关卡
    /// </summary>
    public string QuestionID { get; set; } = "";
    public string Data { get; set; } = "{}";
    public long TimeFrame { get; set; }
    /// <summary>
    /// 进度
    /// </summary>
    public ushort State { get; set; }
}

public class TreasureBoxOpenData
{
    public string Token { get; set; } = "";
    public string TreasureBoxId { get; set; } = "";
}

public class UserLevelUpData
{
    public string Token { get; set; } = "";
}