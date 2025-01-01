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
    /// <returns></returns>
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