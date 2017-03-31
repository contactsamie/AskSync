using System;
using Akka.Actor;

namespace AskSync.AkkaAskSyncLib
{
    internal class CacheFactory
    {
        internal static SynchronizedCache<string, Tuple<IActorRef, object>> Cache = new SynchronizedCache<string, Tuple<IActorRef, object>>();
    }
}