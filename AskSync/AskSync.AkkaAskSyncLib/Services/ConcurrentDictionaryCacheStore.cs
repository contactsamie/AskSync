using System;
using System.Collections.Concurrent;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class ConcurrentDictionaryCacheStore
    {
        protected static readonly ConcurrentDictionary<string, Tuple<IActorRef, object>> _cache = new ConcurrentDictionary<string, Tuple<IActorRef, object>>();
    }
}