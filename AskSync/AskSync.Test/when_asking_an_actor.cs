using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;
using Akka.Util.Internal;
using AskSync.AkkaAskSyncLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskSync.Test
{
    [TestClass]
    public class when_asking_an_actor
    {
        public const int TotalCounter = 100;
        [TestMethod]
        public void use_ask_sync()
        {
            var system = ActorSystem.Create("TestCAtorSystem");
            var testActor = system.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
           
            var list = Enumerable.Range(1, TotalCounter).ToList();
            var result=new ConcurrentDictionary<string, ActorIdentity>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(list, (i) =>
            {
                var res = testActor.AskSync<ActorIdentity>(new Identify(null),null,i.ToString());
                result[i.ToString()] = res;
            });
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            foreach (var data in list.Select(i => result[i.ToString()]))
            {
                Assert.IsTrue(data != null);
            }
        }

        [TestMethod]
        public void use_ask_async()
        {
          
            var system = ActorSystem.Create("TestCAtorSystem");
            var testActor = system.ActorOf(Props.Create<TestActor>(), nameof(TestActor));

            var list = Enumerable.Range(1, TotalCounter).ToList();
            var result = new ConcurrentDictionary<string, ActorIdentity>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(list, (i) =>
            {
                var res = testActor.Ask<ActorIdentity>(new Identify(null)).Result;
                result[i.ToString()] = res;
            });
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            foreach (var data in list.Select(i => result[i.ToString()]))
            {
                Assert.IsTrue(data != null);
            }
        }
    }
}
