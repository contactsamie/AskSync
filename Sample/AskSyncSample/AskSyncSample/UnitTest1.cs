using System;
using System.Threading.Tasks;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskSyncSample
{
    public class TestActor : ReceiveActor
    {
        public TestActor()
        {
            var time = DateTime.UtcNow;
            ReceiveAny(_ =>
            {
                Sender.Tell(time);
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
            const string config = @"
                akka { 
                    actor {
                        
                    }
                    remote {
                        helios.tcp {                            
                            transport-protocol = tcp
                            port = 10000
                            hostname = {0}
                            send-buffer-size = 512000b
                            receive-buffer-size = 512000b
                            maximum-frame-size = 1024000b
                            tcp-keepalive = on
                        }

                        transport-failure-detector {
                            heartbeat-interval = 60 s # default 4s
                            acceptable-heartbeat-pause = 20 s # default 10s
                        }
                    }

                    stdout-loglevel = DEBUG
                    loglevel = DEBUG

                    debug {  
                            receive = on 
                            autoreceive = on
                            lifecycle = on
                            event-stream = on
                            unhandled = on
                    }
                }
                ";
            var systemA = ActorSystem.Create("TestActorSystemA");
            var systemB = ActorSystem.Create("TestActorSystemB", config);
            systemB.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
            var remoteAddress = "akka.tcp://TestActorSystemB@localhost:10000/user/" + nameof(TestActor);
            var resultAskSync = systemA.ActorSelection(remoteAddress).AskSync<DateTime>("", TimeSpan.FromSeconds(3));

            var resultAsk = Task.Run(()=> systemA.ActorSelection(remoteAddress).Ask<DateTime>("", TimeSpan.FromSeconds(3))).Result;
            Console.WriteLine(resultAsk);
            Console.WriteLine(resultAskSync);
            Assert.AreEqual(resultAskSync,resultAsk);
        }



    }
}
