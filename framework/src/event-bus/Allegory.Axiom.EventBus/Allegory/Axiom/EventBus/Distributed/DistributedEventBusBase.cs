using System.Threading.Tasks;
using Allegory.Axiom.DependencyInjection;
using Allegory.Axiom.UnitOfWork;
using Microsoft.Extensions.Options;

namespace Allegory.Axiom.EventBus.Distributed;

public abstract class DistributedEventBusBase(
    IUnitOfWorkManager unitOfWorkManager,
    DistributedEventHandlerFactory factory,
    IOptions<DistributedEventBusOptions> options)
    : IDistributedEventBus, ISingletonService
{
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected DistributedEventHandlerFactory Factory { get; } = factory;
    public DistributedEventBusOptions Options { get; } = options.Value;

    public virtual async Task PublishAsync<T>(
        T payload,
        DistributedEventPublishMode publishMode = DistributedEventPublishMode.Auto)
        where T : notnull
    {
        if (publishMode == DistributedEventPublishMode.Immediate)
        {
            await PublishToMessageBrokerAsync(payload);
            return;
        }

        if (UnitOfWorkManager.Current == null)
        {
            if (publishMode == DistributedEventPublishMode.OnUnitOfWorkComplete)
            {
                await PublishToMessageBrokerAsync(payload);
            }
            else
            {
                await PublishToOutboxAsync(payload);
            }

            return;
        }

        // When unit of work exists
        if (publishMode == DistributedEventPublishMode.OnUnitOfWorkComplete)
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.AfterComplete,
                () => PublishToMessageBrokerAsync(payload));
        }
        else
        {
            UnitOfWorkManager.Current.AddHook(
                UnitOfWorkHookPoint.BeforeComplete,
                () => PublishToOutboxAsync(payload));
        }
    }

    protected virtual Task PublishToOutboxAsync<T>(T payload) where T : notnull
    {
        //Save to store

        return Task.CompletedTask;
    }

    protected abstract Task PublishToMessageBrokerAsync<T>(T payload) where T : notnull;

    public abstract Task InitializeAsync();
}