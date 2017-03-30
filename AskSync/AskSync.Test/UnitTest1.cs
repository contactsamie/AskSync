using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;
using Akka.Util.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskSync.Test
{
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


    public class TestActorA : ActorBase
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

        protected override void Unhandled(object message)
        {
            base.Unhandled(message);
        }

        protected override bool Receive(object message)
        {
            throw new NotImplementedException();
        }
    }
}
