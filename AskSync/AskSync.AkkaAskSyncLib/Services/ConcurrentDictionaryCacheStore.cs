using System;
using System.Collections.Concurrent;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Services
{
    [Obsolete("this was way slower")]
    internal class ConcurrentDictionaryCacheStore
    {
        protected static readonly ConcurrentDictionary<string, Tuple<AskMessage, object>> Cache =
            new ConcurrentDictionary<string, Tuple<AskMessage, object>>();
    }
}