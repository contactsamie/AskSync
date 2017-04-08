using System;
using System.Threading;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Actors;
using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class DefaultAskSynchronously : IAskSynchronously
    {
        public T AskSyncInternal<T>(
            IActorRef workerActor
            , ICanTell actoRef
            , object whatToAsk
            , TimeSpan? timeout
            , string id
            , int workerActorPoolSize
            , SynchronousAskFactory synchronousAskFactory
            , int? retryIdentificationCount
            , bool identifyBeforeSending
            , Func<int, TimeSpan> calculateTimeBeforeRetry
            )
        {
            id = id ?? Guid.NewGuid().ToString();
            var signal = new ManualResetEventSlim();
            var message = new AskMessage(
                id
                , actoRef
                , whatToAsk
                , signal
                , workerActorPoolSize
                , retryIdentificationCount
                , identifyBeforeSending
                , calculateTimeBeforeRetry);
            workerActor.Tell(message);
            signal.Wait(timeout ?? TimeSpan.FromSeconds(10));
            signal.Dispose();
            var result = synchronousAskFactory.GetCacheService().Read(id).Item2;
            if (result == null)
            {
                throw new Exception($"Possibly timeout of {timeout} exceeded");
            }
            return (T) result;
        }
    }
}