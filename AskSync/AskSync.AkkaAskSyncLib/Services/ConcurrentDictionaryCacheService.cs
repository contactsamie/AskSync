using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Contracts;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Services
{
    [Obsolete("this was way slower")]
    internal class ConcurrentDictionaryCacheService : ConcurrentDictionaryCacheStore, ICacheService
    {
        public void AddOrUpdate(string id, AskMessage actorRef, object messageReturned)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (actorRef == null) throw new ArgumentNullException(nameof(actorRef));
            var newValue = new Tuple<AskMessage, object>(actorRef, messageReturned);
            Cache.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
        }

        public Tuple<AskMessage, object> Read(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            var data = Cache[id];
            return data;
        }
    }
}