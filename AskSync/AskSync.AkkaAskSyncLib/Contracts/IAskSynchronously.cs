using System;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib.Contracts
{
    internal interface IAskSynchronously
    {
        T AskSyncInternal<T>(
            IActorRef workerActor
            , ICanTell actoRef
            , object whatToAsk
            , TimeSpan? timeout
            , string id
            , int workerActorPoolSize
            , SynchronousAskFactory synchronousAskFactory
            , int? retryIdentificationCount
            , bool identifyBeforeSending
            , Func<int, TimeSpan> calculateTimeBeforeRetry);
    }
}