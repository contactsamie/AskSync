using System;
using System.Configuration;
using Akka.Actor;
using FamilyCluster.Common;

namespace FamilyCluster.Brother
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting BrotherSystem ...");
            using (var system = ActorSystem.Create("BrotherSystem"))
            {
                var client = ConfigurationManager.AppSettings["client"];
                var brotherEchoActor = system.ActorOf(Props.Create(() => new EchoActor()), "BrotherEchoActor");

                while (true)
                {
                    var message = Console.ReadLine();
                    brotherEchoActor.Tell(new Hello("From BrotherSystem to BrotherEchoActor" + message),
                        ActorRefs.NoSender);

                    system.ActorSelection(client).Tell(new Hello("From BrotherSystem to client at " + client + message));
                }
            }
        }
    }
}