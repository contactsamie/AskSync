using System;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib.Contracts
{
    internal interface IAskSynchronously
    {
        T AskSyncInternal<T>(IActorRef workerActor, ICanTell actoRef, object whatToAsk, TimeSpan? timeout, string id, SynchronousAskFactory SynchronousAskFactory);
    }
}