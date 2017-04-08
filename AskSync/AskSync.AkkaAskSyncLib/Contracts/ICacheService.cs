using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Contracts
{
    internal interface ICacheService
    {
        void AddOrUpdate(string id, AskMessage actorRef, object messageReturned);

        Tuple<AskMessage, object> Read(string id);
    }
}