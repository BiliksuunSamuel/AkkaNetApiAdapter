using System.Reflection;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.DependencyInjection;
using AkkaNetApiAdapter.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaNetApiAdapter.Extensions;

public static class ServicesCollectionsExtension
{
  
    /// <summary>
    /// Register Actor System
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <param name="actorTypes"></param>
    /// <param name="subscriptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddActorSystem(
        this IServiceCollection services,
        Action<ActorConfig> configure,
        Type[] actorTypes,
        params (Type ActorType, Type MessageType)[] subscriptions)
    {
        services.Configure(configure);
        var actorConfig = new ActorConfig();
        configure.Invoke(actorConfig);

        var actorSystemName = Regex.Replace(Assembly.GetExecutingAssembly().GetName().Name ?? "ActorSystemName",
            @"[^a-zA-Z\s]+", "");

        services.AddSingleton(sp =>
        {
            var actorSystemSetup = BootstrapSetup
                .Create()
                .And(DependencyResolverSetup
                    .Create(sp));

            var actorSystem = ActorSystem
                .Create(actorSystemName, actorSystemSetup);

            // Register the actors dynamically
            foreach (var actorType in actorTypes)
            {
                if (!typeof(BaseActor).IsAssignableFrom(actorType)) continue;

                var method = typeof(TopLevelActors)
                    .GetMethod(nameof(TopLevelActors.RegisterActor))!
                    .MakeGenericMethod(actorType);
                method.Invoke(null, new object[] { actorSystem, actorType.Name });
            }

            TopLevelActors.ActorSystem = actorSystem;

            // Subscribe to the event stream
            foreach (var (actorType, messageType) in subscriptions)
            {
                if (!typeof(BaseActor).IsAssignableFrom(actorType)) continue;

                var actorRef = typeof(TopLevelActors)
                    .GetMethod(nameof(TopLevelActors.GetActor), BindingFlags.Static | BindingFlags.Public)
                    ?.MakeGenericMethod(actorType)
                    .Invoke(null, new object[] { });

                if (actorRef == null) continue;

                var subscribeMethod = actorSystem.EventStream
                    .GetType()
                    .GetMethod(nameof(actorSystem.EventStream.Subscribe))
                    ?.MakeGenericMethod(messageType);
                subscribeMethod?.Invoke(actorSystem.EventStream, new[] { actorRef, messageType });
            }
            return actorSystem;
        });

        return services;
    }
    
    
}

