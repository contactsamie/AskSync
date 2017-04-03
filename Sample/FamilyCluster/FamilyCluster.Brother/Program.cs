using System;
using System.Configuration;
using Akka.Actor;
using AskSync.AkkaAskSyncLib;
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
                   var result= system.ActorSelection(client).AskSync(new Hello("From BrotherSystem to client at " + client + message), null, new AskSyncOptions() { UseDefaultRemotingActorSystemConfig = true });
                    Console.WriteLine(result);
                }
            }
        }
    }
}