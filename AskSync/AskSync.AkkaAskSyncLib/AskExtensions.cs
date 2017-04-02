using Akka.Actor;
using System;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
        internal static ActorSystem ActorSystem { set; get; }
        internal static SynchronousAskFactory SynchronousAskFactory = new SynchronousAskFactory();
        
        public static T AskSync<T>(
            this ICanTell actorRef
          , object whatToAsk
          , TimeSpan? timeout = null
          , string id = null)
        {
            ActorSystem = ActorSystem ?? ActorSystem.Create("AskSyncActorSystem-" + Guid.NewGuid());
            var cacheService = SynchronousAskFactory.GetCacheService();
            var synchronousAsk = SynchronousAskFactory.GetSynchronousAsk();
            return synchronousAsk.AskSyncInternal<T>(ActorSystem, actorRef, whatToAsk, timeout, id, SynchronousAskFactory);
        }
    }
}