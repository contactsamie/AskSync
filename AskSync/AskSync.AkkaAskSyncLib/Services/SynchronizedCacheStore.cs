using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib.Common;

namespace AskSync.AkkaAskSyncLib.Services
{
    internal class SynchronizedCacheStore
    {
        protected static readonly SynchronizedCache<string, Tuple<IActorRef, object>> Cache = new SynchronizedCache<string, Tuple<IActorRef, object>>();
    }
}