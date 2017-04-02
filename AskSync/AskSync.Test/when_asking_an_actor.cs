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
        public const int TotalCounter = 1000;
        private readonly List<int> _list;
        private const int GeneralExpectedNumberOfExecutionPerSeconds = 70;
        private readonly Func<int, TimeSpan?> _getMaxExpectedDuration = (expectedNumberOfExecutionPerSeconds) =>
        {
            var result = TimeSpan.FromMilliseconds((int) (TotalCounter*1000/expectedNumberOfExecutionPerSeconds) );
            return result;
        };
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

        /*
         * for SynchronizedCacheService
         took 00:00:07.8825096 : Expected 50 ex/s but 
         actual rate is 126.863150284016 ex/s : . 
         It took 7882.5096ms 
         instead of 20000ms for 1000 calls.
         */
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
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(50));

        }

        /*
         for SynchronizedCacheService
            
            took 00:00:01.1628254 : 
            Expected 300.0300030003 ex/s but 
            actual rate is 859.974334925948 ex/s : . 
            It took 1162.8254ms 
            instead of 3333ms for 1000 calls.
             */
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
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(300));

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
            var duration = AssertMeetsExpectation(sw, _list, _result,_getMaxExpectedDuration(50));
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
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(1000));
        }

        private TimeSpan AssertMeetsExpectation(
            Stopwatch sw
          , IEnumerable<int> list
          , IDictionary<string, ActorIdentity> result
          , TimeSpan? maxExpectedDuration = null)
        {
            sw.Stop();
            var expected = (maxExpectedDuration ?? _getMaxExpectedDuration(GeneralExpectedNumberOfExecutionPerSeconds)) ?? TimeSpan.MaxValue;
             string report = $"Expected {TotalCounter/ expected.TotalSeconds} ex/s but actual rate is {TotalCounter/ sw.Elapsed.TotalSeconds} ex/s : . It took {sw.Elapsed.TotalMilliseconds}ms instead of {expected.TotalMilliseconds}ms for {TotalCounter} calls.";
            _output.WriteLine($"took {sw.Elapsed} : {report}");
            foreach (var data in list.Select(i => result[i.ToString()]))
            {
                Assert.True(data != null);
            }
            Assert.True(sw.Elapsed < expected, $"Execution succeeded but took longer - {report} ");

            return sw.Elapsed;
        }
    }
}