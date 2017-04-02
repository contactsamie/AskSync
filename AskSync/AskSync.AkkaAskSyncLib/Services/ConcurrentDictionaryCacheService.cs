using Akka.Actor;
using AskSync.AkkaAskSyncLib.Contracts;
using System;

namespace AskSync.AkkaAskSyncLib.Services
{
    [Obsolete("this was way slower")]
    internal class ConcurrentDictionaryCacheService : ConcurrentDictionaryCacheStore, ICacheService
    {
        public void AddOrUpdate(string id, IActorRef actorRef, object messageReturned)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (actorRef == null) throw new ArgumentNullException(nameof(actorRef));
            var newValue = new Tuple<IActorRef, object>(actorRef, messageReturned);
            Cache.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
        }

        public Tuple<IActorRef, object> Read(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var data = Cache[id];
            return data;
        }
    }


}