using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/test-1")]
public class Test1 : ControllerBase
{
    [HttpGet]
    public async Task GetAsync() {}
}