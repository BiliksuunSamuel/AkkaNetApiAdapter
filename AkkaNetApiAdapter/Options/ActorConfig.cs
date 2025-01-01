namespace AkkaNetApiAdapter.Options;

public abstract class BaseActorConfig
{
    public int NumberOfInstances { get; set; } = 10;
    public int UpperBound { get; set; } = 100;
}

public class ActorConfig
{
    public SendCallbackActorConfig SendCallbackActorConfig { get; set; } = new();
}

public class SendCallbackActorConfig : BaseActorConfig
{
}