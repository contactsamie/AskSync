using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Internal;
using AskSync.AkkaAskSyncLib;
using Xunit;
using Xunit.Abstractions;

namespace AskSync.Test
{
    public class when_asking_an_actor
    {
        public const int TotalCounter = 1000;
        private const int GeneralExpectedNumberOfExecutionPerSeconds = 70;

        private readonly Func<int, TimeSpan?> _getMaxExpectedDuration = expectedNumberOfExecutionPerSeconds =>
        {
            var result = TimeSpan.FromMilliseconds(TotalCounter * 1000 / expectedNumberOfExecutionPerSeconds);
            return result;
        };

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
        public void use_ask_sync_serial_with_warming()
        {
            var warmup = _testActorRef.AskSync(new Identify(null), new AskSyncOptions() { WorkerActorPoolSize = 1000 });

            var sw = new Stopwatch();
            sw.Start();
            _list.ForEach(i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString() });
                _result[i.ToString()] = res;
            });
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(15000));
        }
        /*     
        took 00:00:00.0588610 : 
        Expected 3508.77192982456 ex/s but actual 
        rate is 16989.1778936817 ex/s : . 
        It took 58.861ms instead of 285ms for 1000 calls.
        */
        [Fact]
        public void use_ask_sync_serial()
        {

            var sw = new Stopwatch();
            sw.Start();
            _list.ForEach(i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString() });
                _result[i.ToString()] = res;
            });
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(3500));
        }
        [Fact]
        public void use_ask_sync_serial_in_comparison_tofaster_option()
        {
            var warmup = _testActorRef.AskSync(new Identify(null));

            var noOfWorkers = 100;

            var sw = new Stopwatch();
            sw.Start();
            _list.ForEach(i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString() });
                _result[i.ToString()] = res;
            });
            var durationSlow = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(1800));



            var sw1 = new Stopwatch();
            sw1.Start();
            _list.ForEach(i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString(), WorkerActorPoolSize = noOfWorkers });
                _result[i.ToString()] = res;
            });
            var durationFast = AssertMeetsExpectation(sw1, _list, _result, _getMaxExpectedDuration(3500));


            /*
             REPORT-------------------
             noOfWorkers : 1000
             252 ms SLOW
             47 ms FAST
             Margin : 204 ms             
             */
            _output.WriteLine("REPORT-------------------");
            _output.WriteLine("noOfWorkers : " + noOfWorkers);
            _output.WriteLine(durationSlow.Milliseconds + " ms SLOW");
            _output.WriteLine(durationFast.Milliseconds + " ms FAST");
            _output.WriteLine("Margin : " + (durationSlow - durationFast).Milliseconds + " ms");

            Assert.True(durationSlow.TotalMilliseconds - durationFast.TotalMilliseconds > 0);
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
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString() });
                _result[i.ToString()] = res;
            });
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(200));
        }

        [Fact]
        public void use_ask_sync_parallel_remoting()
        {
            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(_list, i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString(), UseDefaultRemotingActorSystemConfig = true });
                _result[i.ToString()] = res;
            });
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(150));// used to be 300
        }



        [Fact]
        public void use_ask_sync_serial_remoting()
        {
            var sw = new Stopwatch();
            sw.Start();
            _list.ForEach(i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(new Identify(null), null,
                    new AskSyncOptions { ExecutionId = i.ToString(), UseDefaultRemotingActorSystemConfig = true });
                _result[i.ToString()] = res;
            });
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(700));
        }



        private TimeSpan AssertMeetsExpectation(
            Stopwatch sw
            , IEnumerable<int> list
            , IDictionary<string, ActorIdentity> result
            , TimeSpan? maxExpectedDuration = null)
        {
            sw.Stop();
            var expected = (maxExpectedDuration ?? _getMaxExpectedDuration(GeneralExpectedNumberOfExecutionPerSeconds)) ??
                           TimeSpan.MaxValue;
            string report =
                $"Expected {TotalCounter / expected.TotalSeconds} ex/s but actual rate is {TotalCounter / sw.Elapsed.TotalSeconds} ex/s : . It took {sw.Elapsed.TotalMilliseconds}ms instead of {expected.TotalMilliseconds}ms for {TotalCounter} calls.";
            _output.WriteLine($"took {sw.Elapsed} : {report}");
            foreach (var data in list.Select(i => result[i.ToString()]))
            {
                Assert.True(data != null);
            }
            Assert.True(sw.Elapsed < expected, $"Execution succeeded but took longer - {report} ");

            return sw.Elapsed;
        }




        [Fact]
        public void use_ask_async_parallel()
        {
            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(_list, i =>
            {
                var res = _testActorRef.Ask<ActorIdentity>(new Identify(null)).Result;
                _result[i.ToString()] = res;
            });
            var duration = AssertMeetsExpectation(sw, _list, _result, _getMaxExpectedDuration(50));
        }

        [Fact]
        public void use_ask_async_serial()
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

        [Fact]
        public void load_test_sortof()
        {
            const int size = 100000;
            var results = new ConcurrentDictionary<string, ActorIdentity>();
            var range = Enumerable.Range(1, size).ToList();
            var sw = new Stopwatch();
            sw.Start();
            range.ForEach(i =>
            {
                var res = _testActorRef.AskSync<ActorIdentity>(
                    new Identify(null)
                    , TimeSpan.FromMinutes(1)
                    , new AskSyncOptions()
                    {
                        ExecutionId = i.ToString()
                    });
                results[i.ToString()] = res;
            });
            _output.WriteLine($"took {sw.Elapsed.TotalMilliseconds} ms : to send {size} messages");
            foreach (var data in range.Select(i => results[i.ToString()]))
            {
                Assert.True(data != null && data.Subject != null);
            }
            Assert.Equal(size, results.Count);
            _output.WriteLine("Verified!");
        }

        [Fact]
        public void it_should_do_ask_sync()
        {
            var system = ActorSystem.Create("TestActorSystem");
            var actorRef = system.ActorOf(Props.Create(() => new TestActor()), nameof(TestActor) + "Tmp");
            var result = actorRef.AskSync<ActorIdentity>(new Identify(null));
            Assert.True(result!=null && result.Subject!=null);
        }
        [Fact]
        public void it_should_do_ask_sync2()
        {
            const string actorAddress = "user/" + nameof(TestActor) + "Tmp";
            var system = ActorSystem.Create("TestActorSystem");
            var actorRef = system.ActorOf(Props.Create(() => new TestActor()), nameof(TestActor) + "Tmp");
            var result = system.ActorSelection(actorAddress).AskSync<ActorIdentity>(new Identify(null));
            Assert.True(result != null && result.Subject != null);
        }
        [Fact]
        public void it_should_fail_to_do_ask_sync_when_not_enough_retry_option_is_specified()
        {
            const string actorAddress = "user/" + nameof(TestActor) + "Tmp";
            var system = ActorSystem.Create("TestActorSystem");
            
            var result = system.ActorSelection("vjhhjjhfj").AskSync<ActorIdentity>(new Identify(null), new AskSyncOptions()
            {
                CalculateTimeBeforeRetry = (count) =>
                {
                    system.ActorOf(Props.Create(() => new TestActor()), nameof(TestActor) + "Tmp");
                    return TimeSpan.Zero;
                }
            });
            Assert.False(result != null && result.Subject != null);
        }

        [Fact]
        public void it_should_fail_to_do_ask_sync_when_not_enough_retry_option_is_specified2()
        {
            const string actorAddress = "user/" + nameof(TestActor) + "Tmp";
            var system = ActorSystem.Create("TestActorSystem");

            var result = system.ActorSelection(actorAddress).AskSync<ActorIdentity>(new Identify(null), new AskSyncOptions()
            {
                CalculateTimeBeforeRetry = (count) =>
                {
                    system.ActorOf(Props.Create(() => new TestActor()), nameof(TestActor) + "Tmp");
                    return TimeSpan.Zero;
                }
                ,IdentifyBeforeSending = true
            });
            Assert.True(result != null && result.Subject != null);
        }
        [Fact]
        public void it_should_fail_to_do_ask_sync_when_not_enough_retry_option_is_specified3()
        {
            const string actorAddress = "user/" + nameof(TestActor)+"Tmp";
            var system = ActorSystem.Create("TestActorSystem");

            var result = system.ActorSelection(actorAddress).AskSync<ActorIdentity>(new Identify(null), new AskSyncOptions()
            {
                CalculateTimeBeforeRetry = (count) =>
                {
                    system.ActorOf(Props.Create(() => new TestActor()), nameof(TestActor) + "Tmp");
                    return TimeSpan.Zero;
                },
                IdentifyBeforeSending = true,
                RetryIdentificationCount = 2
            });
            Assert.True(result != null && result.Subject != null);
        }
    }
}