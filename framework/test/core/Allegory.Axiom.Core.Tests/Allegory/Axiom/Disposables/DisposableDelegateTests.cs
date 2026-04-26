using System;
using System.Threading.Tasks;
using Xunit;

namespace Allegory.Axiom.Disposables;

public class DisposableDelegateTests
{
    [Fact]
    public async Task Test()
    {
        await using var _ = new AsyncDisposableDelegate(() => ValueTask.CompletedTask);
        
        await Task.Delay(100, TestContext.Current.CancellationToken);
    }

    public async Task Test2()
    {
        int i = 0;
        await Task.Delay(100);
        i++;
    }
}
