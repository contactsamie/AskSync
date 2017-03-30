using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;
using Akka.Util.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskSync.Test
{
    public class CacheFactory
    {
        public static SynchronizedCache<string, Tuple<IActorRef, object>> Cache = new SynchronizedCache<string, Tuple<IActorRef, object>>();

    }

    public static class AskExtensions
    {

        public static T AskSync<T>(this ActorSystem actorSystem, IActorRef iCantell, object whatToAsk, TimeSpan? timeout = null,string id=null )
        {
             Actor = Actor?? actorSystem.ActorOf(Props.Create(() => new TestActorA()), nameof(TestActorA));
             id =id?? Guid.NewGuid().ToString();
            
            Actor.Tell(new AskMessage() { messageId = id, Actor = iCantell, Message= whatToAsk });
            SpinWait.SpinUntil(() => CacheFactory.Cache.Contains(id) && CacheFactory.Cache.Read(id).Item2 != null, 10000);
            var res =(T) CacheFactory.Cache.Read(id).Item2 ;
            return res;
        }

        public static IActorRef Actor { get; set; }
    }



    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var totalCounter = 1000;
            var system = ActorSystem.Create("TestCAtorSystem");
            var actorB = system.ActorOf(Props.Create<TestActorB>(), nameof(TestActorB));
           
            var list = Enumerable.Range(1, totalCounter).ToList();
            var result=new ConcurrentDictionary<string,string>();
            Parallel.ForEach(list, (i) =>
            {
                var res = system.AskSync<string>(actorB, new Identify(null),null,i.ToString());
                
                result.AddOrUpdate(i.ToString(),res,(a,b)=>res);
            });
            foreach (var i in list)
            {
                Assert.IsTrue(result[i.ToString()]!=null);
            }
        }
    }


    public class TestActorA : ReceiveActor
    {
        public TestActorA()
        {
            Receive<AskMessage>(message =>
            {
                CacheFactory. Cache.Add(message.messageId, new Tuple<IActorRef, object>(message.Actor,null));
                message.Actor.Tell(message.Message);
            });
            //todo get messageId
            Receive<ReliableDeliveryEnvelope>(message =>
            {
                var cache = CacheFactory.Cache.Read(id);
                CacheFactory.Cache.AddOrUpdate(id, new Tuple<IActorRef, object>(cache.Item1, message.Subject));
            });
        }
    }

    public class AskMessage
    {
        public string messageId;
        public IActorRef Actor;
        public object Message { get; set; }
    }


    public class TestActorB : ReceiveActor
    {
    }

    /// <summary>
    /// Represents a lock that is used to manage access to a resource, allowing multiple threads for reading or exclusive access for writing.
    /// Ensure that TVal overrides Equals which will be used in implementing AddOrUpdate
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TVal"></typeparam>
    public class SynchronizedCache<TKey,TVal>
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<TKey, TVal> innerCache = new Dictionary<TKey, TVal>();

        public int Count
        { get { return innerCache.Count; } }

        public TVal Read(TKey key)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache[key];
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public void Add(TKey key, TVal value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(key, value);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public bool AddWithTimeout(TKey key, TVal value, int timeout)
        {
            if (cacheLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    innerCache.Add(key, value);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public AddOrUpdateStatus AddOrUpdate(TKey key, TVal value)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                TVal result = default(TVal);
                if (innerCache.TryGetValue(key, out result))
                {
                    if (Equals(result, value))
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        cacheLock.EnterWriteLock();
                        try
                        {
                            innerCache[key] = value;
                        }
                        finally
                        {
                            cacheLock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(key, value);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        public void Delete(TKey key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };

        ~SynchronizedCache()
        {
            if (cacheLock != null) cacheLock.Dispose();
        }

        public bool Contains(TKey id)
        {
        return    innerCache.ContainsKey(id);
        }
    }
}
