using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.IO;
using Akka.Util.Internal;
using AskSync.AkkaAskSyncLib;
using Xunit;
using Xunit.Abstractions;

namespace AskSync.Test
{
   
    public class when_asking_an_actor
    {
       
        private readonly ITestOutputHelper _output=null;

        public when_asking_an_actor(ITestOutputHelper output)
        {
            this._output = output;
        }
        public const int TotalCounter = 100;
        [Fact]
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
            AssertMeetsExpectation(sw, list, result);
        }

       

        [Fact]
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
            AssertMeetsExpectation(sw, list, result);
        }
        private void AssertMeetsExpectation(Stopwatch sw, List<int> list, ConcurrentDictionary<string, ActorIdentity> result)
        {
            sw.Stop();
            _output.WriteLine(sw.Elapsed.ToString());
            foreach (var data in list.Select(i => result[i.ToString()]))
            {
                Assert.True(data != null);
            }
        }
    }
}
