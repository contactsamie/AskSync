using System;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    public static class AskExtensions
    {
        public static ActorSystem ActorSystem { set; get; }

        public static T AskSync<T>(this IActorRef iCantell, object whatToAsk, TimeSpan? timeout = null, string id = null)
        {
            ActorSystem = ActorSystem ?? ActorSystem.Create("AskSyncACtorSystem-" + Guid.NewGuid());
            return new AskSynchronously().AskSyncInternal<T>(ActorSystem, iCantell, whatToAsk, timeout, id);
        }
    }
}