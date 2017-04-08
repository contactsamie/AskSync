using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class SynchronizedCacheService : SynchronizedCacheStore, ICacheService
    {
        public void AddOrUpdate(string id, AskMessage actorRef, object messageReturned)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (actorRef == null) throw new ArgumentNullException(nameof(actorRef));
            Cache.AddOrUpdate(id, new Tuple<AskMessage, object>(actorRef, messageReturned));
        }

        public Tuple<AskMessage, object> Read(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var data = Cache.Read(id);

            return data;
        }
    }
}