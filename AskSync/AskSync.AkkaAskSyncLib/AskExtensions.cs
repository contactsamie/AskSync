using System;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
        internal static ActorSystem ActorSystem { set; get; }
        internal static AskSynchronously AskSynchronously { set; get; }

        public static T AskSync<T>(
            this IActorRef actorRef
          , object whatToAsk
          , TimeSpan? timeout = null
          , string id = null)
        {
            ActorSystem = ActorSystem ?? 
                ActorSystem.Create("AskSyncActorSystem-" + Guid.NewGuid());
            return (AskSynchronously ?? new AskSynchronously())
                .AskSyncInternal<T>(ActorSystem, actorRef, whatToAsk,timeout, id);
        }
    }
}