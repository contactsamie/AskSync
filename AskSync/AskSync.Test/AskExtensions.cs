using System;
using System.Threading;
using Akka.Actor;

namespace AskSync.Test
{
    public static class AskExtensions
    {

        public static T AskSync<T>(this ActorSystem actorSystem, IActorRef iCantell, object whatToAsk, TimeSpan? timeout = null,string id=null )
        {
            Actor = Actor?? actorSystem.ActorOf(Props.Create(() => new TestActorA()), nameof(TestActorA));
            id =id?? Guid.NewGuid().ToString();
            
            Actor.Tell(new AskMessage() { messageId = id, Actor = iCantell, Message= whatToAsk });
            SpinWait.SpinUntil(() => CacheFactory.Cache.Contains(id) && CacheFactory.Cache.Read(id).Item2 != null, 10000);
            var res =(T) CacheFactory.Cache.Read(id).Item2 ;
            return res;
        }

        public static IActorRef Actor { get; set; }
    }
}