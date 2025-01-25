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


/// <summary>
/// The upper parameter in the ActorResizer class represents the maximum number of routes (actor instances) that can be created. This value sets an upper limit on the number of actors that can be spawned to handle the workload. The resizer will not create more than this number of routes,even if the system is under high load
/// Default Value for UpperBound is 100
/// Default Value for NumberOfInstances is 10
/// </summary>
public class ActorResizer
{
    
    public int NumberOfInstances { get; set; } = 10;
    public int UpperBound { get; set; } = 100;
}