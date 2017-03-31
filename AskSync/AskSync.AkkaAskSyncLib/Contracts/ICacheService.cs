using Akka.Actor;
using System;

namespace AskSync.AkkaAskSyncLib.Contracts
{
    internal interface ICacheService
    {
        void AddOrUpdate(string id, IActorRef actorRef, object messageReturned);

        Tuple<IActorRef, object> Read(string id);
    }
}