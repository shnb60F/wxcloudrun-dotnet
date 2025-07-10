using Microsoft.AspNetCore.Mvc;

namespace aspnetapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(DateTime.Now + ":" + "Hello");
    }
}