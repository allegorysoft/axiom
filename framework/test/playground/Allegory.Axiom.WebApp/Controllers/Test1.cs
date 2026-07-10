using System.Threading.Tasks;
using Allegory.Axiom.EventBus.Distributed;
using Events.Event1;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("api/test-1")]
public class Test1(IDistributedEventBus distributedEventBus) : ControllerBase
{
    protected IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;

    [HttpGet("{name}")]
    public virtual async Task GetAsync(string name)
    {
        await DistributedEventBus.PublishAsync(new Event1(name));
    }
}