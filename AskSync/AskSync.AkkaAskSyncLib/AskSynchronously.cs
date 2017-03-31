using System;
using System.Threading;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    internal class AskSynchronously
    {
        internal T AskSyncInternal<T>(
           ActorSystem actorSystem
         , IActorRef actoRef
         , object whatToAsk
         , TimeSpan? timeout = null
         , string id = null
        )
        {
            id = id ?? Guid.NewGuid().ToString();
            var actor = actorSystem.ActorOf(Props.Create(() => new AskSyncReceiveActor()));
            var message = new AskMessage(id, actoRef, whatToAsk, new ManualResetEventSlim());
            actor.Tell(message);
            message.Signal.Wait(timeout ?? TimeSpan.FromSeconds(3));
            message.Signal.Dispose();
            var result = (T) CacheFactory.Cache.Read(id).Item2;
            return result;
        }
    }
}