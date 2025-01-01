using System.Reflection;
using System.Text.RegularExpressions;
using Akka.Actor;
using Akka.DependencyInjection;
using AkkaNetApiAdapter.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AkkaNetApiAdapter.Extensions;

public static class ServicesCollectionsExtension
{
    //allow this method to accept actor classes to be registered
    public static IServiceCollection AddActorSystem(
        this IServiceCollection services,
        Action<ActorConfig> configure)
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
            
            TopLevelActors.ActorSystem = actorSystem;
            return actorSystem;
        });

        return services;
    }
    
    
}