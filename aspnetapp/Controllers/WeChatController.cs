using System.Reflection;
using aspnetapp;
using aspnetapp.EF;
using aspnetapp.Entity;
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
    readonly EnhancementSelectionService enhancementSelectionService;
    readonly TreasureBoxService treasureBoxService;

    Assembly assembly = Assembly.GetExecutingAssembly();

    public WeChatController(GuardersService guardersService, GuardersSessionManager guardersSessionManager, JWTUtil jwtUtil, EnhancementSelectionService enhancementSelectionService, TreasureBoxService treasureBoxService)
    {
        this.guardersService = guardersService;
        this.guardersSessionManager = guardersSessionManager;
        this.jwtUtil = jwtUtil;
        this.enhancementSelectionService = enhancementSelectionService;
        this.treasureBoxService = treasureBoxService;
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
    public async Task<IActionResult> Logoff([FromBody] OnlyToken logOffData, [FromHeader] string userId)
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

    [HttpPost("guarder/getall")]
    public async Task<IActionResult> GuarderGetAll([FromBody] OnlyToken onlyToken, [FromHeader] string userId)
    {
        ICollection<GuarderDB> guarderDBs = await guardersService.GuarderGetAll(userId);

        JArray treasureBoxArray = new JArray();

        foreach (var guarderDB in guarderDBs)
        {
            JObject treasureBoxObject = new JObject
            {
                { "GuarderType", (int)guarderDB.GuarderType },
                { "GuarderLevel", guarderDB.GuarderLevel },
                { "GuarderKakera", guarderDB.GuarderKakera }
            };

            treasureBoxArray.Add(treasureBoxObject);
        }

        JObject pairs = new JObject
        {
            { "GuarderDBs", treasureBoxArray }
        };

        return Ok(pairs.ToString());
    }

    [HttpPost("treasurebox/getall")]
    public async Task<IActionResult> TreasureBoxGetAll([FromBody] OnlyToken getAllData, [FromHeader] string userId)
    {
        ICollection<TreasureBoxDB> treasureBoxDBs = await treasureBoxService.GetAll(userId);

        JArray treasureBoxArray = new JArray();

        foreach (var treasureBoxDB in treasureBoxDBs)
        {
            JObject treasureBoxObject = new JObject
            {
                { "TreasureBoxId", treasureBoxDB.TreasureBoxId },
                { "TreasureBoxType", (int)treasureBoxDB.TreasureBoxType }
            };

            treasureBoxArray.Add(treasureBoxObject);
        }

        JObject pairs = new JObject
        {
            { "TreasureBoxDBs", treasureBoxArray }
        };

        return Ok(pairs.ToString());
    }

    [HttpPost("user/getall")]
    public async Task<IActionResult> UserGetAll([FromBody] OnlyToken onlyToken, [FromHeader] string userId)
    {
        UserDB userDB = await guardersService.UserGetAll(userId);

        JObject userObject = new JObject
        {
            { "UserName", userDB.UserName },
            { "UserXP", userDB.UserXP },
            { "UserLevel", userDB.UserLevel },
            { "UserGold", userDB.UserGold },
            { "UserAP", userDB.UserAP },
            { "UserCreatedAt", userDB.UserCreatedAt }
        };

        return Ok(userObject.ToString());
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
    public async Task<IActionResult> QuestionCreate([FromBody] QuestionCreateData questionCreateData, [FromHeader] string userId)
    {
        string questionID = questionCreateData.QuestionID;
        GuardersSession guardersSession = new GuardersSession { DeadLine = DateTime.Now.AddMinutes(60), UserId = userId, QuestionID = questionID, TimeFrame = 0 };
        List<GuarderDB> guarderDBs = await guardersService.GuarderSelect(userId);

        foreach (var guarderDB in guarderDBs)
        {
            string key = $"{guarderDB.GuarderType}";
            string className = $"aspnetapp.Entity.{guarderDB.GuarderType}";
            // 根据类名获取类型信息
            Type type = assembly.GetType(className);
            if (type == null)
            {
                throw new Exception($"{userId} {questionCreateData.QuestionID}: Type '{className}' not found.");
            }
            IEntity entity = (IEntity)Activator.CreateInstance(type);
            ((Entity)entity).AdjustForLevel((short)guarderDB.GuarderLevel);
            guardersSession.AddEntity(key, entity);
        }
        guardersSession.Init();
        guardersSessionManager.AddSession((userId, questionID), guardersSession);
        return Ok();
    }

    [HttpPost("question/getenhancements")]
    public IActionResult QuestionGetEnhancements([FromBody] QuestionGetEnhancementData questionGetEnhancementData, [FromHeader] string userId)
    {
        string questionID = questionGetEnhancementData.QuestionID;

        try
        {
            GuardersSession guardersSession = guardersSessionManager.GetSession((userId, questionID));
            EnhancementType[] enhancementTypes = enhancementSelectionService.GetEnhancements(guardersSession);
            guardersSessionManager.SetEnhancement((userId, questionID), enhancementTypes);
            Console.WriteLine("QuestionGetEnhancements:" + enhancementTypes.ToString());
            return Ok(enhancementTypes);
        }
        catch (System.Exception)
        {
            return BadRequest("the session is expire");
        }
        // return Ok(new EnhancementType[] { EnhancementType.ArcherAIDown1p, EnhancementType.ArcherDamageUp1p, EnhancementType.ArcherPiercingUp5p });
    }

    [HttpPost("question/enhancement")]
    public IActionResult QuestionEnhancement([FromBody] QuestionEnhancementsData questionEnhancementsData, [FromHeader] string userId)
    {
        string questionID = questionEnhancementsData.QuestionID;

        try
        {
            if (!guardersSessionManager.UpdateEnhancement((userId, questionID), questionEnhancementsData.enhancementType))
                return BadRequest("the session is not ready");
            Console.WriteLine("QuestionEnhancement:" + questionEnhancementsData.enhancementType.ToString());
            return Ok();
        }
        catch (System.Exception)
        {
            return BadRequest("the session is expire");
        }
    }

    [HttpPost("question/clear")]
    public async Task<IActionResult> QuestionClear([FromBody] QuestionClearData questionClearData, [FromHeader] string userId)
    {
        string questionID = questionClearData.QuestionID;
        string data = questionClearData.Data;
        long timeFrame = questionClearData.TimeFrame;
        if (string.IsNullOrEmpty(questionID))
            return BadRequest("QuestionID is null or empty");

        GuardersSession guardersSession = new GuardersSession { UserId = userId, QuestionID = questionID, TimeFrame = timeFrame };
        try
        {
            guardersSession.ReadJson(data);
        }
        catch (Exception)
        {
            guardersSessionManager.RemoveSession((userId, questionID));
            Console.WriteLine($"fail: QuestionClear: JSON Data is null");
            return BadRequest("JSON Data is null");
        }

        if (guardersSessionManager.SessionClear((userId, questionID), guardersSession))
        {
            await guardersService.QuestionClear(
                new QuestionDB { UserId = userId, QuestionID = questionID, QuestionStar = 3, QuestionBestTime = (int)timeFrame },
                treasureBoxService.GetBox(userId),
                1);
            guardersSessionManager.RemoveSession((userId, questionID));
            return Ok();
        }
        else
        {
            guardersSessionManager.RemoveSession((userId, questionID));
            return BadRequest("Failed to clear question.");
        }
    }

    [HttpPost("treasurebox/getbox")]
    public async Task<IActionResult> GetBox([FromHeader] string userId)
    {
        TreasureBoxDB treasureBoxDB = treasureBoxService.GetBox(userId);
        try
        {
            await treasureBoxService.AddBox(treasureBoxDB);
        }
        catch (NotFiniteNumberException exception)
        {
            return BadRequest(exception.Message);
        }
        JObject pairs = new JObject();
        pairs["TreasureBoxId"] = treasureBoxDB.TreasureBoxId;
        pairs["TreasureBoxType"] = (int)treasureBoxDB.TreasureBoxType;
        return Ok($"{pairs}");
    }

    [HttpPost("treasurebox/openbox")]
    public async Task<IActionResult> OpenBox([FromBody] TreasureBoxOpenData treasureBoxOpenData, [FromHeader] string userId)
    {
        (long, long) gohobiProperty = await treasureBoxService.OpenBox(treasureBoxOpenData.TreasureBoxId);
        JObject pairs = new JObject();
        pairs["UserGold"] = gohobiProperty.Item1;
        pairs["GuarderKakera"] = (int)gohobiProperty.Item2;
        return Ok($"{pairs}");
    }

    [HttpPost("user/levelup")]
    public async Task<IActionResult> UserLevelUp([FromBody] OnlyToken userLevelUpData, [FromHeader] string userId)
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

public class OnlyToken
{
    public string Token { get; set; } = "";
}

public class LoginData
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
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

public class QuestionEnhancementsData
{
    public string Token { get; set; } = "";
    public string QuestionID { get; set; } = "";
    public EnhancementType enhancementType { get; set; }
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