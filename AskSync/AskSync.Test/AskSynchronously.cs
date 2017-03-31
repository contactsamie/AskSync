using System;
using System.Threading;
using Akka.Actor;

namespace AskSync.Test
{
    internal class AskSynchronously
    {
        internal T AskSyncInternal<T>(ActorSystem actorSystem, IActorRef iCantell, object whatToAsk, TimeSpan? timeout = null, string id = null)
        {
            id = id ?? Guid.NewGuid().ToString();
            var actor = actorSystem.ActorOf(Props.Create(() => new AskSyncReceiveActor()));
            var message = new AskMessage()
            {
                MessageId = id,
                Actor = iCantell,
                Message = whatToAsk,
                Signal = new ManualResetEventSlim()
            };
            actor.Tell(message);
            message.Signal.Wait();
            message.Signal.Dispose();
            var read = CacheFactory.Cache.Read(id).Item2;
            var res = (T)read;
            return res;
        }
    }
}