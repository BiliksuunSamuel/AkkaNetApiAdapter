using System.Reflection;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.DependencyInjection;
using AkkaNetApiAdapter.HelperMethods;
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
         (Type actorType, int? numberOfInstances,int?upperBound)[] actorTypes,
         (Type ActorType, Type MessageType)[] subscriptions)
    {
        services.Configure(configure);
        var actorConfig = new ActorConfig();
        configure.Invoke(actorConfig);

        var actorSystemName = Regex.Replace(
            Assembly.GetExecutingAssembly().GetName().Name ?? "ActorSystemName",
            @"[^a-zA-Z\s]+", "");

        services.AddScoped<IAkkaActorEventService, AkkaActorEventService>();

        services.AddSingleton(sp =>
        {
            var actorSystemSetup = BootstrapSetup
                .Create()
                .And(DependencyResolverSetup.Create(sp));

            var actorSystem = ActorSystem.Create(actorSystemName, actorSystemSetup);

            // Register the actors dynamically
            foreach (var actorType in actorTypes)
            {
                if (!typeof(BaseActor).IsAssignableFrom(actorType.actorType)) continue;

                var registerWithRouter = actorType.numberOfInstances.HasValue && actorType.upperBound.HasValue;

                // Call RegisterActor for each actor type dynamically
                var method = registerWithRouter
                    ? typeof(TopLevelActors)
                        .GetMethod(nameof(TopLevelActors.RegisterActorWithRouter))!
                        .MakeGenericMethod(actorType.actorType)
                    : typeof(TopLevelActors)
                        .GetMethod(nameof(TopLevelActors.RegisterActor))!
                        .MakeGenericMethod(actorType.actorType); // Dynamically make the method generic
                // Invoke RegisterActor with the actor system and optionally an actor name
                if (!registerWithRouter)
                {
                    method.Invoke(null, new object[] { actorSystem, actorType.actorType.Name });
                }
                else
                {
                    method.Invoke(null,
                        new object[]
                        {
                            actorSystem,
                            actorType.numberOfInstances!,
                            actorType.upperBound!,
                            actorType.actorType.Name,
                        });
                }

            }

            TopLevelActors.ActorSystem = actorSystem;

            // Subscribe to the event stream
            foreach (var (actorType, messageType) in subscriptions)
            {
                if (!typeof(BaseActor).IsAssignableFrom(actorType)) continue;

                var getActorMethod = typeof(TopLevelActors)
                    .GetMethod(nameof(TopLevelActors.GetActor))
                    !.MakeGenericMethod(actorType);

                var actorRef =
                    getActorMethod.Invoke(null, new object[] { actorType.Name }); // Pass empty string as default name

                if (actorRef == null) continue;
                // Subscribe the actor to the event stream
                actorSystem.EventStream.Subscribe((IActorRef)actorRef, messageType);
            }

            return actorSystem;
        });

        return services;
    }


}

