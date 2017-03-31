using Akka.Actor;
using System;

namespace AskSync.AkkaAskSyncLib.Contracts
{
    internal interface IAskSynchronously
    {
        T AskSyncInternal<T>(
            ActorSystem actorSystem
            , IActorRef actoRef
            , object whatToAsk
            , TimeSpan? timeout
            , string id
            , SynchronousAskFactory SynchronousAskFactory
        );
    }
}