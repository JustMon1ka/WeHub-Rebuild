using Microsoft.AspNetCore.Mvc;
using MyBackend.ORCLEUPDATER;

namespace MyBackend.Controllers;

[ApiController]
[Route("orcleupdater")]
public class TestDataController : ControllerBase
{
    private readonly TestDataService _service;
    public TestDataController(TestDataService service) => _service = service;

    [HttpPost("insert")]
    public IActionResult Insert()
    {
        _service.InsertTestData();
        return Ok(new { message = "测试数据已插入" });
    }

    [HttpPost("delete")]
    public IActionResult Delete()
    {
        _service.DeleteTestData();
        return Ok(new { message = "测试数据已删除" });
    }
}
