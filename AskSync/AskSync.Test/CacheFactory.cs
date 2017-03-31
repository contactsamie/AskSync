using System;
using Akka.Actor;

namespace AskSync.Test
{
    internal class CacheFactory
    {
        internal static SynchronizedCache<string, Tuple<IActorRef, object>> Cache = new SynchronizedCache<string, Tuple<IActorRef, object>>();
    }
}