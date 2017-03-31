using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AskSync.Test
{
    public class when_asking_an_actor
    {
        public const int TotalCounter = 10000;
        private readonly List<int> _list;

        private readonly ITestOutputHelper _output;
        private readonly ConcurrentDictionary<string, ActorIdentity> _result;
        private readonly IActorRef _testActorRef;

        public when_asking_an_actor(ITestOutputHelper output)
        {
            _output = output;
            var system = ActorSystem.Create("TestActorSystem");
            _testActorRef = system.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
            _list = Enumerable.Range(1, TotalCounter).ToList();
            _result = new ConcurrentDictionary<string, ActorIdentity>();
        }

        [Fact]
        public void use_ask_sync_parallel()
        {
            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(_list, i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null, i.ToString());
                _result[i.ToString()] = res;
            });
            var maxExpectedDuration = TimeSpan.FromSeconds(8);
            var duration = AssertMeetsExpectation(sw, _list, _result);
            Assert.True(duration < maxExpectedDuration, $"Execution succeeded but took longer than expected . It took {duration.TotalMilliseconds}ms instead of {maxExpectedDuration.TotalMilliseconds}ms for {TotalCounter} calls");

        }

        /// <summary>
        /// duration 00:00:07.7591367
        /// </summary>
        [Fact]
        public void use_ask_sync_serial()
        {
            var sw = new Stopwatch();
            sw.Start();
            _list.ForEach(i =>
           {
               var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null, i.ToString());
               _result[i.ToString()] = res;
           });
            var maxExpectedDuration = TimeSpan.FromSeconds(8);
            var duration = AssertMeetsExpectation(sw, _list, _result);
            Assert.True(duration < maxExpectedDuration, $"Execution  succeeded but took longer than expected . It took {duration.TotalMilliseconds}ms instead of {maxExpectedDuration.TotalMilliseconds}ms for {TotalCounter} calls");

        }

        [Fact]
        public void use_ask_async_serial()
        {
            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(_list, i =>
            {
                var res = _testActorRef.Ask<ActorIdentity>(new Identify(null)).Result;
                _result[i.ToString()] = res;
            });
            var maxExpectedDuration = TimeSpan.FromSeconds(8);
            var duration = AssertMeetsExpectation(sw, _list, _result);
            Assert.True(duration < maxExpectedDuration, $"Execution  succeeded but took longer than expected . It took {duration.TotalMilliseconds}ms instead of {maxExpectedDuration.TotalMilliseconds}ms for {TotalCounter} calls");

        }

        [Fact]
        public void use_ask_async_parallel()
        {
            var sw = new Stopwatch();
            sw.Start();
            _list.ForEach(i =>
           {
               var res = _testActorRef.Ask<ActorIdentity>(new Identify(null)).Result;
               _result[i.ToString()] = res;
           });
            var maxExpectedDuration = TimeSpan.FromSeconds(8);
            var duration = AssertMeetsExpectation(sw, _list, _result);
            Assert.True(duration < maxExpectedDuration, $"Execution  succeeded but took longer than expected . It took {duration.TotalMilliseconds}ms instead of {maxExpectedDuration.TotalMilliseconds}ms for {TotalCounter} calls");
        }

        private TimeSpan AssertMeetsExpectation(
            Stopwatch sw
          , IEnumerable<int> list
          , IDictionary<string, ActorIdentity> result)
        {
            sw.Stop();
            _output.WriteLine(sw.Elapsed.ToString());
            foreach (var data in list.Select(i => result[i.ToString()]))
            {
                Assert.True(data != null);
            }
            return sw.Elapsed;
        }
    }
}