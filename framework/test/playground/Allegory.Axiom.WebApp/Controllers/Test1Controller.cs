using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Events;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/test-1")]
public class Test1Controller(IDistributedEventBus distributedEventBus) : ControllerBase
{
    protected IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;

    [HttpGet("{number:int}")]
    public virtual async Task GetAsync(int number)
    {
        await DistributedEventBus.PublishAsync(new Event1(number));
    }
}