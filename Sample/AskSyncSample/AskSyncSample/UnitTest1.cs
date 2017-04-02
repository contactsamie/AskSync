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

        protected override void PreRestart(Exception reason, object message)
        {
            base.PreRestart(reason, message);
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
             Func<int,string> config =(p)=>
                 $@"
                akka {{ 
                    actor {{
                        
                    }}
                    remote {{
                        helios.tcp {{                            
                            transport-protocol = tcp
                            port = {p}
                            hostname = 0.0.0.0
                            public-hostname = localhost
                            #send-buffer-size = 512000b
                            #receive-buffer-size = 512000b
                            #maximum-frame-size = 1024000b
                            #tcp-keepalive = on
                        }}
                        transport-failure-detector {{
                            #heartbeat-interval = 60 s # default 4s
                            #acceptable-heartbeat-pause = 20 s # default 10s
                        }}
                    }}

                    #stdout-loglevel = DEBUG
                    #loglevel = DEBUG

                    debug {{  
                            receive = on 
                            autoreceive = on
                            lifecycle = on
                            event-stream = on
                            unhandled = on
                    }}
                }}";

            var remotePort = 1000;
            var remoteACtorSystem = "TestActorSystemB";
            var systemB = ActorSystem.Create(remoteACtorSystem, config(remotePort));
            var actorB=  systemB.ActorOf(Props.Create<TestActor>(), nameof(TestActor));
          
            var systemA = ActorSystem.Create("TestActorSystemA", config(0));
            var remoteAddress = "akka.tcp://"+ remoteACtorSystem + "@localhost:"+ remotePort + "/user/*/" + nameof(TestActor);
            Task.Delay(TimeSpan.FromSeconds(2)).Wait();
            var resultAsk = Task.Run(() => systemA.ActorSelection(remoteAddress).Ask<DateTime>("123", TimeSpan.FromSeconds(3))).Result;
            var resultAskSync = systemA.ActorSelection(remoteAddress).AskSync<DateTime>("123", TimeSpan.FromSeconds(3));
            Console.WriteLine(resultAsk);
            Console.WriteLine(resultAskSync);
            Assert.AreEqual(resultAskSync,resultAsk);
        }



    }
}
