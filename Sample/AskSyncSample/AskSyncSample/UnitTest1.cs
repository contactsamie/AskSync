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
    }
}
