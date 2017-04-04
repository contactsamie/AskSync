using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Contracts;

namespace AskSync.AkkaAskSyncLib.Services
{
    [Obsolete("this was way slower")]
    internal class ConcurrentDictionaryCacheService : ConcurrentDictionaryCacheStore, ICacheService
    {
        public void AddOrUpdate(string id, ICanTell actorRef, object messageReturned)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (actorRef == null) throw new ArgumentNullException(nameof(actorRef));
            var newValue = new Tuple<ICanTell, object>(actorRef, messageReturned);
            Cache.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
        }

        public Tuple<ICanTell, object> Read(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var data = Cache[id];
            return data;
        }
    }
}