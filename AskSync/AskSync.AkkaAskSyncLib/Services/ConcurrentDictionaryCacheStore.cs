using System;
using System.Collections.Concurrent;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib.Services
{
    [Obsolete("this was way slower")]
    internal class ConcurrentDictionaryCacheStore
    {
        protected static readonly ConcurrentDictionary<string, Tuple<IActorRef, object>> Cache = new ConcurrentDictionary<string, Tuple<IActorRef, object>>();
    }
}