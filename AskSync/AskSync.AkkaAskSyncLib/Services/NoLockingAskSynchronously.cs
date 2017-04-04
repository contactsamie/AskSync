using System;
using System.Threading;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Actors;
using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Services
{
    [Obsolete("its slower")]
    internal class NoLockingAskSynchronously : IAskSynchronously
    {
        public T AskSyncInternal<T>(
            ActorSystem actorSystem
            , ICanTell actoRef
            , object whatToAsk
            , TimeSpan? timeout
            , string id
            , SynchronousAskFactory synchronousAskFactory
            )
        {
            id = id ?? Guid.NewGuid().ToString();
            var signal = new ManualResetEventSlim();
            // var resultData = new ResultData();
            var actor =
                actorSystem.ActorOf(Props.Create(() => new AskSyncReceiveActor(synchronousAskFactory /*,resultData*/)));
            var message = new AskMessage(id, actoRef, whatToAsk, signal);
            actor.Tell(message);
            signal.Wait(timeout ?? TimeSpan.FromSeconds(3));
            signal.Dispose();
            // return (T)resultData.Result;
            return default(T);
        }
    }
}