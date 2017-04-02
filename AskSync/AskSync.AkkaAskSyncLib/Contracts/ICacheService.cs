using Akka.Actor;
using System;

namespace AskSync.AkkaAskSyncLib.Contracts
{
    internal interface ICacheService
    {
        void AddOrUpdate(string id, ICanTell actorRef, object messageReturned);

        Tuple<ICanTell, object> Read(string id);
    }
}