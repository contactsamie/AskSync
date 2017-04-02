using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Contracts;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class SynchronizedCacheService : SynchronizedCacheStore, ICacheService
    {
        public void AddOrUpdate(string id, ICanTell actorRef, object messageReturned)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (actorRef == null) throw new ArgumentNullException(nameof(actorRef));
            Cache.AddOrUpdate(id, new Tuple<ICanTell, object>(actorRef, messageReturned));
        }

        public Tuple<ICanTell, object> Read(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var data = Cache.Read(id);

            return data;
        }
    }
}