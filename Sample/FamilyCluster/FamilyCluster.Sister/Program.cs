using System;
using System.Configuration;
using Akka.Actor;
using FamilyCluster.Common;

namespace FamilyCluster.Sister
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting SisterSystem ...");
            using (var system = ActorSystem.Create("SisterSystem"))
            {
                var client = ConfigurationManager.AppSettings["client"];
                var sisterEchoActor = system.ActorOf(Props.Create(() => new EchoActor()), "SisterEchoActor");

                while (true)
                {
                    var message = Console.ReadLine();
                    sisterEchoActor.Tell(new Hello("From SisterSystem to sisterEchoActor" + message), ActorRefs.NoSender);

                    system.ActorSelection(client).Tell(new Hello("From SisterSystem to client at " + client + message));
                }
            }
        }
    }
}