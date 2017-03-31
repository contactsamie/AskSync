using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
using Xunit;
using Xunit.Abstractions;

namespace AskSync.Test
{
    public class when_asking_an_actor
    {
        public const int TotalCounter = 100;
        private readonly List<int> _list;

        private readonly ITestOutputHelper _output;
        private readonly ConcurrentDictionary<string, ActorIdentity> _result;
        private readonly ActorSystem _system;
        private readonly IActorRef _testActorRef;

        public when_asking_an_actor(ITestOutputHelper output)
        {
            _output = output;
            _system = ActorSystem.Create("TestActorSystem");
            _testActorRef = _system.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
            _list = Enumerable.Range(1, TotalCounter).ToList();
            _result = new ConcurrentDictionary<string, ActorIdentity>();
            
        }

        [Fact]
        public void use_ask_sync()
        {
            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(_list, i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null, i.ToString());
                _result[i.ToString()] = res;
            });
            AssertMeetsExpectation(sw, _list, _result);
        }

        [Fact]
        public void use_ask_async()
        {
            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(_list, i =>
            {
                var res = _testActorRef.Ask<ActorIdentity>(new Identify(null)).Result;
                _result[i.ToString()] = res;
            });
            AssertMeetsExpectation(sw, _list, _result);
        }

        private void AssertMeetsExpectation(
            Stopwatch sw
          , List<int> list
          , ConcurrentDictionary<string, ActorIdentity> result)
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