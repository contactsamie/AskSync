using System;
using System.Linq;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
using Xunit;

namespace AskSync.Test
{
    public class RemotingTest
    {
        private const int NumberOfMessages = 10;
        private const bool UseRemoting = true;
        private readonly ActorSystem _brotherSystem;
        private readonly ActorSystem _sisterSystem;
        private readonly TimeSpan? _timeOut = TimeSpan.FromSeconds(10);

        public RemotingTest()
        {
            _brotherSystem = UseRemoting
                ? ActorSystem.Create(BrotherActorSystemName, GetHoconSettingForRemoting(BrotherPortAddress))
                : ActorSystem.Create(BrotherActorSystemName);

            _sisterSystem = UseRemoting
                ? ActorSystem.Create(SisterActorSystemName, GetHoconSettingForRemoting(SisterPortAddress))
                : ActorSystem.Create(SisterActorSystemName);
        }

        [Fact]
        public void for_regular_ask_with_direct_reference_to_remote_actor()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at  {brotherActorRef.Path.Address} {i}");
                var result = brotherActorRef.Ask<Hello>(message, _timeOut).Result;
                Assert.Equal(message.Message, result.Message);
            }
        }

        [Fact]
        public void for_askSync_with_direct_reference_to_remote_actor()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at   {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorRef.AskSync<Hello>(message, _timeOut, new AskSyncOptions
                {
                    ActorSystemConfig = GetHoconSettingForRemoting(0)
                });
                Assert.Equal(message.Message, result.Message);
            }
        }

        [Fact]
        public void for_askSync_with_remote_actor_obetained_from_actor_selection()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorSelection.AskSync<Hello>(message, _timeOut,
                    new AskSyncOptions
                    {
                        ExistingActorSystem = _sisterSystem
                    });
                Assert.Equal(message.Message, result.Message);
            }
        }


        [Fact]
        public void for_askSync_with_remote_actor_obetained_from_actor_selection2()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);
            var brotherActorRefResolvedFromSisterActorSystem =
                brotherActorSelection.ResolveOne(TimeSpan.FromSeconds(5)).Result;

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorRefResolvedFromSisterActorSystem.AskSync<Hello>(message, _timeOut,
                    new AskSyncOptions
                    {
                        ExistingActorSystem = _sisterSystem
                    });
                Assert.Equal(message.Message, result.Message);
            }
        }


        [Fact]
        public void for_askSync_with_remote_actor_obetained_from_actor_selection_ignoring_system_config()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorSelection.AskSync<Hello>(message, _timeOut,
                    new AskSyncOptions
                    {
                        ActorSystemConfig = GetHoconSettingForRemoting(0),
                        ExistingActorSystem = _sisterSystem
                    });
                Assert.Equal(message.Message, result.Message);
            }
        }


        [Fact]
        public void for_askSync_with_remote_actor_obetained_from_actor_selection2_ignoring_system_config()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);
            var brotherActorRefResolvedFromSisterActorSystem =
                brotherActorSelection.ResolveOne(TimeSpan.FromSeconds(5)).Result;

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorRefResolvedFromSisterActorSystem.AskSync<Hello>(message, _timeOut,
                    new AskSyncOptions
                    {
                        ActorSystemConfig = GetHoconSettingForRemoting(0),
                        ExistingActorSystem = _sisterSystem
                    });
                Assert.Equal(message.Message, result.Message);
            }
        }

        [Fact]
        public void for_askSync_with_remote_actor_obetained_from_actor_selection3()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);
            var brotherActorRefResolvedFromSisterActorSystem =brotherActorSelection.ResolveOne(TimeSpan.FromSeconds(5)).Result;

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorRefResolvedFromSisterActorSystem.AskSync<Hello>(message, _timeOut, _sisterSystem);
                Assert.Equal(message.Message, result.Message);
            }
        }

        [Fact]
        public void for_askSync_with_remote_actor_obetained_from_actor_selection4()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);
            var brotherActorRefResolvedFromSisterActorSystem = brotherActorSelection.ResolveOne(TimeSpan.FromSeconds(5)).Result;

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result =(Hello) brotherActorRefResolvedFromSisterActorSystem.AskSync(message, _timeOut, _sisterSystem);
                Assert.Equal(message.Message, result.Message);
            }
        }
        [Fact]
        public void for_askSync_get_actor_identity()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);
            var brotherActorRefResolvedFromSisterActorSystem = brotherActorSelection.ResolveOne(TimeSpan.FromSeconds(5)).Result;

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at {brotherActorRef.Path.Address}  {i}");
                var result = brotherActorRefResolvedFromSisterActorSystem.AskSync<ActorIdentity>(new Identify(null), _sisterSystem);
                Assert.True(result.Subject != null);
            }
        }
        [Fact]
        public void for_regular_ask_with_remote_actor_obetained_from_actor_selection()
        {
            var brotherActorRef = _brotherSystem.ActorOf(Props.Create(() => new EchoActor()), BrotherActorName);
            var brotherActorSelection = _sisterSystem.ActorSelection(BrotherAddress);
            var brotherActorRefResolvedFromSisterActorSystem =
                brotherActorSelection.ResolveOne(TimeSpan.FromSeconds(5)).Result;

            foreach (var i in Enumerable.Range(0, NumberOfMessages))
            {
                var message = new Hello($"From SisterSystem to client at  {brotherActorRef.Path.Address} {i}");
                var result = brotherActorRefResolvedFromSisterActorSystem.Ask<Hello>(message, _timeOut).Result;
                Assert.Equal(message.Message, result.Message);
            }
        }

        #region Test Setups

        private const int BrotherPortAddress = 20000;
        private const int SisterPortAddress = 0;
        private const string SisterActorName = "SisterEchoActor";
        private const string BrotherActorName = "BrotherEchoActor";
        private const string BrotherActorSystemName = "BrotherSystem";
        private const string SisterActorSystemName = "SisterSystem";


        private static readonly string BrotherAddress =
            $"akka.tcp://{BrotherActorSystemName}@localhost:{BrotherPortAddress}/user/{BrotherActorName}";

        private static string _sisterAddress =
            $"akka.tcp://{SisterActorSystemName}@localhost:{SisterPortAddress}/user/{SisterActorName}";

        private static readonly Func<int, string> GetHoconSettingForRemoting =
            port =>
                $@"
                akka {{ 
                     actor{{ 
                        serializers{{ 
                                     hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                                }} 
                  
                        serialization-bindings  {{ 
                                    ""System.Object"" = hyperion
                                }}
                        provider =""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
				    }}
                    remote {{
                        helios.tcp {{
                            port = {port}
                            hostname = localhost
                        }}
                    }}
                }}";

        public class EchoActor : ReceiveActor
        {
            public EchoActor()
            {
                Receive<Hello>(hello =>
                {
                    Console.WriteLine("[{0}]: {1}", Sender, hello.Message);
                    Sender.Tell(hello);
                });
            }
        }

        public class Hello
        {
            public Hello(string message)
            {
                Message = message;
            }

            public string Message { get; }
        }

        #endregion
    }
}