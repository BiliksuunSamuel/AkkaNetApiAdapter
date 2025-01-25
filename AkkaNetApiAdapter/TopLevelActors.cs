using System.Collections.Concurrent;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Routing;
// ReSharper disable All

namespace AkkaNetApiAdapter
{
    public static class TopLevelActors
    {
        private static ConcurrentDictionary<string, IActorRef> _actorRegistry = new();
        private static SupervisorStrategy _defaultSupervisorStrategy = null!;

        /// <summary>
        /// For Publishing Events To All Subscribers
        /// </summary>
        public static ActorSystem? ActorSystem;


        /// <summary>
        /// Get Actor By Name From the Actor Registry
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IActorRef GetActor<T>(string name = "") where T : BaseActor
        {
            _ = name ?? nameof(T);

            string actorFullName = GetActorFullName<T>(name ?? nameof(T));

            if (_actorRegistry.TryGetValue(actorFullName, out var actorInstance))
            {
                return actorInstance;
            }

            throw new ArgumentOutOfRangeException(nameof(actorFullName),
                $"\"{actorFullName}\" not created or registered");
        }


        /// <summary>
        /// Add Actor To the Actor Registry
        /// </summary>
        /// <param name="actorSystem"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RegisterActor<T>(ActorSystem actorSystem, string name = "") where T : BaseActor
        {
            string actorFullName = GetActorFullName<T>(name);

            var actor = CreateNewActor<T>(actorSystem, actorFullName);

            return _actorRegistry.TryAdd(actorFullName, actor);
        }


        /// <summary>
        /// Add Actor With Router To the Actor Registry
        /// </summary>
        /// <param name="actorSystem"></param>
        /// <param name="numberOfInstance"></param>
        /// <param name="upperBound"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool RegisterActorWithRouter<T>(ActorSystem actorSystem, int numberOfInstance, int upperBound,
            string name = "") where T : BaseActor
        {
            if (numberOfInstance >= upperBound)
                throw new ArgumentOutOfRangeException(nameof(numberOfInstance),
                    "numberOfInstance should be >= upperBound");

            string actorFullName = GetActorFullName<T>(name);

            var actor = CreateNewActorWithRouter<T>(actorSystem, numberOfInstance, upperBound, actorFullName);

            return _actorRegistry.TryAdd(actorFullName, actor);
        }

        /// <summary>
        /// Create New Actor
        /// </summary>
        /// <param name="actorSystem"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        private static IActorRef CreateNewActor<T>(ActorSystem actorSystem, string name) where T : BaseActor
        {
            return actorSystem.ActorOf(
                DependencyResolver
                    .For(actorSystem)
                    .Props<T>()
                    .WithSupervisorStrategy(actorSystem.DefaultSupervisorStrategy()), name);
        }

        private static IActorRef CreateNewActorWithRouter<T>(ActorSystem actorSystem, int numberOfInstance,
            int upperBound,
            string name) where T : BaseActor
        {
            return actorSystem.ActorOf(
                DependencyResolver
                    .For(actorSystem)
                    .Props<T>()
                    .WithSupervisorStrategy(actorSystem.DefaultSupervisorStrategy())
                    .WithRouter(new RoundRobinPool(numberOfInstance, new DefaultResizer(numberOfInstance, upperBound))),
                name);
        }

        public static SupervisorStrategy DefaultSupervisorStrategy(this ActorSystem actorSystem)
        {
            return _defaultSupervisorStrategy;
        }

        private static string GetActorFullName<T>(string name) where T : BaseActor
        {
            return $"{name.Trim()}_{typeof(T).Name}";
        }
    }
}