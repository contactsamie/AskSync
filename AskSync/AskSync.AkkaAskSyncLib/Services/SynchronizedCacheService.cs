using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Contracts;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class SynchronizedCacheService : SynchronizedCacheStore, ICacheService
    {
        public void AddOrUpdate(string id, IActorRef actorRef, object messageReturned)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (actorRef == null) throw new ArgumentNullException(nameof(actorRef));
            _cache.AddOrUpdate(id, new Tuple<IActorRef, object>(actorRef, messageReturned));
        }

        public Tuple<IActorRef, object> Read(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var data = _cache.Read(id);

            return data;
        }
    }
}