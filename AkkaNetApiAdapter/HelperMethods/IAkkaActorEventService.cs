namespace AkkaNetApiAdapter.HelperMethods;

public interface IAkkaActorEventService
{
    Task<bool> BroadcastEventAsync<T>(T message);
    Task<bool> SendEventAsync<TP, T>(T message) where TP : BaseActor;
}