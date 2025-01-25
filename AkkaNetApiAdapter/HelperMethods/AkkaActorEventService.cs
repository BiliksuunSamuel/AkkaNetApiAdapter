using Akka.Actor;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AkkaNetApiAdapter.HelperMethods;

public class AkkaActorEventService:IAkkaActorEventService
{
    private readonly ILogger<AkkaActorEventService> _logger;

    public AkkaActorEventService(ILogger<AkkaActorEventService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// This event broadcasts the message to all subscribers of this event  in the actor system 
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<bool> BroadcastEventAsync<T>(T message)
    {
        try
        {
            await Task.CompletedTask;
            _logger.LogDebug("Publishing event: {Event}", JsonConvert.SerializeObject(message, Formatting.Indented));
            if (TopLevelActors.ActorSystem == null)
            {
                _logger.LogError("Actor system is not initialized");
                return false;
            }
            TopLevelActors.ActorSystem.EventStream.Publish(message);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error publishing event: {Event}",
                JsonConvert.SerializeObject(message, Formatting.Indented));
            return false;
        }
    }

    /// <summary>
    /// Send event to a specific actor
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="TP"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<bool> SendEventAsync<TP, T>(T message) where TP : BaseActor
    {
        try
        {
            await Task.CompletedTask;
            var actorName = typeof(TP).Name;
            _logger.LogDebug("Sending event: {event} to actor: {actorName}",
                JsonConvert.SerializeObject(message, Formatting.Indented), actorName);
            var actor = TopLevelActors.GetActor<TP>(actorName);
            actor.Tell(message, ActorRefs.NoSender);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending event: {Event} to actor: {ActorName}",
                JsonConvert.SerializeObject(message, Formatting.Indented), typeof(TP).Name);
            return false;
        }
    }
    
    
    /// <summary>
    /// Get response from an actor after sending an event
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="TP"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TTp"></typeparam>
    /// <returns></returns>
    public async Task<TTp?> ActorAskAsync<TP,T,TTp>(T message) where TP : BaseActor
    {
        try
        {
            await Task.CompletedTask;
            var actorName = typeof(TP).Name;
            _logger.LogDebug("Sending event: {event} to actor: {actorName}",
                JsonConvert.SerializeObject(message, Formatting.Indented), actorName);
            var actor = TopLevelActors.GetActor<TP>(actorName);
            var result = await actor.Ask<TTp>(message);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending event: {Event} to actor: {ActorName}",
                JsonConvert.SerializeObject(message, Formatting.Indented), typeof(TP).Name);
            return default;
        }
    }
}