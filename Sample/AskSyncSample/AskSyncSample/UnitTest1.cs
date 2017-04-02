using System;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskSyncSample
{
    public class TestActor : ReceiveActor
    {
        public TestActor()
        {
            ReceiveAny(_ =>
            {
                Sender.Tell(DateTime.UtcNow);
            });
        }
    }
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var system = ActorSystem.Create("TestActorSystem");
            var testActor = system.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
            Console.WriteLine(testActor.AskSync<DateTime>(""));
        }


        [TestMethod]
        public void Remoting()
        {
            var config = @"
                akka {{  
                    actor {{
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }}
                    remote {{
                        helios.tcp {{
                            transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                            applied-adapters = []
                            transport-protocol = tcp
                            port = 0
                            hostname = {0}
                            send-buffer-size = 512000b
                            receive-buffer-size = 512000b
                            maximum-frame-size = 1024000b
                            tcp-keepalive = on
                        }}

                        transport-failure-detector {{
                            heartbeat-interval = 60 s # default 4s
                            acceptable-heartbeat-pause = 20 s # default 10s
                        }}
                    }}

                    stdout-loglevel = DEBUG
                    loglevel = DEBUG

                    debug {{  
                            receive = on 
                            autoreceive = on
                            lifecycle = on
                            event-stream = on
                            unhandled = on
                    }}
                }}
                ";
            var systemA = ActorSystem.Create("TestActorSystem", config);
            var testActorA = systemA.ActorOf(Props.Create<TestActor>(), nameof(TestActor));

            var systemB = ActorSystem.Create("TestActorSystem", config);
            var testActorB = systemA.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
            
            Console.WriteLine(systemA.ActorSelection("").AskSync<DateTime>(""));
        }



    }
}
