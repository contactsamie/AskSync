using System;
using Akka.Actor;

namespace AskSync.Test
{
    public class CacheFactory
    {
        public static SynchronizedCache<string, Tuple<IActorRef, object>> Cache = new SynchronizedCache<string, Tuple<IActorRef, object>>();

    }
}