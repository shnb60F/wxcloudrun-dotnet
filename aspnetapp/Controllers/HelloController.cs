using Microsoft.AspNetCore.Mvc;

namespace SimpleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(DateTime.Now + ":" + "Hello");
    }
}