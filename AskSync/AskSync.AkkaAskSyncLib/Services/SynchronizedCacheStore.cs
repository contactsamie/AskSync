using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Common;
using AskSync.AkkaAskSyncLib.Messages;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class SynchronizedCacheStore
    {
        protected static readonly SynchronizedCache<string, Tuple<AskMessage, object>> Cache =
            new SynchronizedCache<string, Tuple<AskMessage, object>>();
    }
}