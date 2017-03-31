using Akka.Actor;
using AskSync.AkkaAskSyncLib.Actors;
using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Messages;
using System;
using System.Threading;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class DefaultAskSynchronously : IAskSynchronously
    {
        public T AskSyncInternal<T>(
           ActorSystem actorSystem
         , IActorRef actoRef
         , object whatToAsk
         , TimeSpan? timeout
         , string id
         , SynchronousAskFactory synchronousAskFactory
        )
        {
            id = id ?? Guid.NewGuid().ToString();
            var signal = new ManualResetEventSlim();
            var resultData = new ResultData();
            var message = new AskMessage(id, actoRef, whatToAsk, signal);
            var actor = actorSystem.ActorOf(Props.Create(() => new AskSyncReceiveActor(synchronousAskFactory, resultData)));
            actor.Tell(message);
            signal.Wait(timeout ?? TimeSpan.FromSeconds(3));
            signal.Dispose();
            var result = synchronousAskFactory.GetCacheService().Read(id).Item2;
            return (T)result;
        }
    }
    internal class NoLockingAskSynchronously : IAskSynchronously
    {
        public T AskSyncInternal<T>(
           ActorSystem actorSystem
         , IActorRef actoRef
         , object whatToAsk
         , TimeSpan? timeout
         , string id
         , SynchronousAskFactory synchronousAskFactory
        )
        {
            id = id ?? Guid.NewGuid().ToString();
            var signal = new ManualResetEventSlim();
            var resultData = new ResultData();
            var message = new AskMessage(id, actoRef, whatToAsk, signal);
            var actor = actorSystem.ActorOf(Props.Create(() => new AskSyncReceiveActor(synchronousAskFactory, resultData)));
            actor.Tell(message);
            signal.Wait(timeout ?? TimeSpan.FromSeconds(3));
            signal.Dispose();
            return (T)resultData.Result;
        }
    }

    internal class ResultData
    {
        public object Result { set; get; }
    }
}